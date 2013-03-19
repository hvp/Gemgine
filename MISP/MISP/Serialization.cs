﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        public static Action<String> ReportSerializationError = null;
        private static void _serializationError(String msg)
        {
            if (ReportSerializationError != null) ReportSerializationError(msg);
        }

        private class ObjectRecord
        {
            internal ScriptObject obj;
            internal int referenceCount;
        }

        private static bool AddRef(ScriptObject obj, List<ObjectRecord> list)
        {
            var spot = list.FirstOrDefault((o) => { return Object.ReferenceEquals(o.obj, obj); });
            if (spot != null) { spot.referenceCount++; return false; }
            else
            {
                list.Add(new ObjectRecord { obj = obj, referenceCount = 1 });
                return true;
            }
        }

        private static void EnumerateObject(
            ScriptObject obj,
            List<ScriptObject> globalFunctions,
            List<ObjectRecord> objects,
            List<ObjectRecord> lambdas)
        {
            if (obj == null) return;

            if (Function.IsFunction(obj))
            {
                //System function?
                if (Function.IsSystemFunction(obj)) return;
                //Lambda function?
                if (globalFunctions.Contains(obj)) return;
                if (AddRef(obj, lambdas))
                    EnumerateObject(obj["declaration-scope"] as ScriptObject, globalFunctions, objects, lambdas);
            }
            else
            {
                foreach (var prop in obj.ListProperties())
                {
                    var value = obj.GetLocalProperty(prop as String);
                    if (value is ScriptObject)
                    {
                        if (AddRef(value as ScriptObject, objects))
                            EnumerateObject(value as ScriptObject, globalFunctions, objects, lambdas);
                    } 
                    else if (value is ScriptList)
                    {
                        foreach (var item in value as ScriptList)
                        {
                            if (item is ScriptObject)
                                if (AddRef(item as ScriptObject, objects))
                                    EnumerateObject(item as ScriptObject, globalFunctions, objects, lambdas);
                        }
                    }

                    
                }
            }
        }

        private static int IndexIn(ScriptObject obj, List<ObjectRecord> list)
        {
            for (int i = 0; i < list.Count; ++i)
                if (list[i].obj == obj) return i;
            return -1;
        }

        private static void EmitObjectProperty(
            System.IO.TextWriter to,
            Object value,
            List<ScriptObject> globalFunctions,
            List<ObjectRecord> objects,
            List<ObjectRecord> lambdas,
            int depth)
        {

            if (value == null) to.Write("null");
            else if (value is ScriptObject)
            {
                if (Function.IsSystemFunction(value as ScriptObject) ||
                    globalFunctions.Contains(value)) to.Write((value as ScriptObject).gsp("@name"));
                else
                {
                    var index = IndexIn(value as ScriptObject, objects);
                    if (index != -1) to.Write("(index objects " + index + ")");
                    else
                    {
                        index = IndexIn(value as ScriptObject, lambdas);
                        if (index != -1) to.Write("(index lambdas " + index + ")");
                        else
                            EmitObject(to, value as ScriptObject, globalFunctions, objects, lambdas, depth);
                    }
                }
            }
            else if (value is String)
                to.Write("\"" + value as String + "\"");
            else if (value is ScriptList)
            {
                to.Write("^(");
                foreach (var item in value as ScriptList)
                {
                    EmitObjectProperty(to, item, globalFunctions, objects, lambdas, depth + 1);
                    to.Write(" ");
                }
                to.Write(")");
            }
            else if (value is Int32)
                to.Write(value.ToString());
            else if (value is UInt32)
                to.Write(value.ToString());
            else if (value is Single)
                to.Write(value.ToString());
            else if (value is IScriptSerializable)
                to.Write((value as IScriptSerializable).Serialize());
            else
            {
                _serializationError("Encountered unserializable type: " + value.GetType().Name);
                to.Write("null");
            }
        }

        private static void EmitObjectProperties(
            System.IO.TextWriter to,
            ScriptObject obj,
            List<ScriptObject> globalFunctions,
            List<ObjectRecord> objects,
            List<ObjectRecord> lambdas,
            int depth)
        {
            foreach (var propertyName in obj.ListProperties())
            {
                to.Write("\n" + new String(' ', depth * 3) + "(" + propertyName as String + " ");
                var value = obj.GetLocalProperty(propertyName as String);
                EmitObjectProperty(to, value, globalFunctions, objects, lambdas, depth);
                to.Write(")");
            }
        }

        private static void EmitObject(
            System.IO.TextWriter to,
            ScriptObject obj,
            List<ScriptObject> globalFunctions,
            List<ObjectRecord> objects,
            List<ObjectRecord> lambdas,
            int depth)
        {
            to.WriteLine("(record ");
            EmitObjectProperties(to, obj, globalFunctions, objects, lambdas, depth + 1);
            to.Write(")");
        }

        private static void EmitObjectRoot(
            System.IO.TextWriter to,
            ScriptObject obj,
            List<ScriptObject> globalFunctions,
            List<ObjectRecord> objects,
            List<ObjectRecord> lambdas)
        {
            var index = IndexIn(obj, objects);
            to.WriteLine("(multi-set (index objects " + index + ") ");
            EmitObjectProperties(to, obj, globalFunctions, objects, lambdas, 1);
            to.Write(")\n");
        }

        private static void emitArgumentSpec(ScriptObject arg, System.IO.TextWriter to)
        {
            if (arg["@mutator"] == null && arg["@optional"] == null && arg["@repeat"] == null && arg["@lazy"] == null)
            {
                to.Write("(arg " + arg["@name"] + ")");
                return;
            }

            if (arg["@mutator"] != null) to.Write("(arg-mutator ");
            if (arg["@optional"] != null) to.Write("(arg-optional ");
            if (arg["@repeat"] != null) to.Write("(arg-repeat ");
            if (arg["@lazy"] != null) to.Write("(arg-lazy ");
            to.Write(arg["@name"]);
            if (arg["@lazy"] != null) to.Write(")");
            if (arg["@repeat"] != null) to.Write(")");
            if (arg["@optional"] != null) to.Write(")");
            if (arg["@mutator"] != null)
            {
                to.Write(" ");
                SerializeCode(to, arg["@mutator"] as ScriptObject);
                to.Write(")");
            }
        }

        public void EmitFunction(ScriptObject func, string type, System.IO.TextWriter to, bool recall = false, int indent = -1)
        {
            to.Write((recall ?  "" : "(") + type + " " + func.gsp("@name") + " ^(");
            var arguments = func["@arguments"] as ScriptList;
            foreach (var arg in arguments)
            {
                if (indent >= 0) to.Write("\n   ");
                emitArgumentSpec(arg as ScriptObject, to);
            }
            if (indent >= 0) to.Write("\n");
            to.Write(") ");
            Engine.SerializeCode(to, func["@function-body"] as ScriptObject, indent);
            if (!recall)
                to.Write((indent >= 0 ? "\n" : "") + " \"" + func.gsp("@help") + "\")\n");
        }

        public void SerializeEnvironment(System.IO.TextWriter to, Scope scope)
        {
            var globalFunctions = new List<ScriptObject>();
            var objects = new List<ObjectRecord>();
            var lambdas = new List<ObjectRecord>();

            //Build list of global non-system functions
            foreach (var func in functions)
            {
                if (Function.IsSystemFunction(func.Value)) continue;
                globalFunctions.Add(func.Value);
            }

            //Filter objects into objects and lambda list
            foreach (var func in globalFunctions)
                EnumerateObject(func["@declaration-scope"] as ScriptObject, globalFunctions, objects, lambdas);
            EnumerateObject(scope, globalFunctions, objects, lambdas);

            //Filter out objects with just a single reference
            objects = new List<ObjectRecord>(objects.Where((o) => { return o.referenceCount > 1; }));
            AddRef(scope, objects);

            to.WriteLine("(lastarg");

            //Emit global functions
            foreach (var func in globalFunctions)
                EmitFunction(func, "defun", to, false, 0);

            //Create and emit lambda functions.
            to.WriteLine("(let ^(\n   ^(\"lambdas\" ^(");
            foreach (var func in lambdas)
            {
                to.Write("      ");
                EmitFunction(func.obj, "lambda", to, false, 0);
            }
            to.WriteLine(")\n   )\n   ^(\"objects\" (array " + objects.Count + " (record)))\n)\n(lastarg\n");

            //Set function declaration scopes.
            foreach (var func in globalFunctions)
            {
                to.WriteLine("(set " + func.gsp("@name") + " \"@declaration-scope\" ");
                EmitObjectProperty(to, func["@declaration-scope"], globalFunctions, objects, lambdas, 1);
                to.WriteLine(")\n");
            }

            for (int i = 0; i < lambdas.Count; ++i)
            {
                var func = lambdas[i];
                to.WriteLine("(set (index lambdas " + i + ") \"@declaration-scope\" ");
                EmitObjectProperty(to, func.obj["@declaration-scope"], globalFunctions, objects, lambdas, 1);
                to.WriteLine(")\n");
            }
            //Emit remaining objects
            foreach (var obj in objects)
                EmitObjectRoot(to, obj.obj, globalFunctions, objects, lambdas);

            //Emit footer
            var thisIndex = IndexIn(scope, objects);
            to.Write("(index objects " + thisIndex + "))))");
        }

    }
}
