﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MISP;

namespace Gem.Gui
{
    public class MispBinding
    {
        private static int _asint(Object o)
        {
            var i = o as int?;
            if (i == null || !i.HasValue) throw new InvalidOperationException("Argument is not an integer.");
            return i.Value;
        }

        private static float _asfloat(Object o)
        {
            var i = o as float?;
            if (i == null || !i.HasValue) return _asint(o);
            return i.Value;
        }

        public static void Bind(MISP.Engine engine)
        {
            var gui = new MISP.GenericScriptObject();
            engine.AddGlobalVariable("gui", (context) => { return gui; });

            gui["rect"] = Function.MakeSystemFunction("rect", Arguments.Args(
                Arguments.Arg("x"), Arguments.Arg("y"), Arguments.Arg("w"), Arguments.Arg("h")),
                "Create a rect.",
                (context, arguments) =>
                {
                    return new Rectangle(_asint(arguments[0]),
                        _asint(arguments[1]),
                        _asint(arguments[2]),
                        _asint(arguments[3]));
                }
            );

            gui["color"] = Function.MakeSystemFunction("color", Arguments.Args(
                Arguments.Arg("r"), Arguments.Arg("g"), Arguments.Arg("b")),
                "Create a color.",
                (context, arguments) =>
                {
                    return new Vector3(_asfloat(arguments[0]), _asfloat(arguments[1]), _asfloat(arguments[2]));
                }
            );

            gui["create"] = MISP.Function.MakeSystemFunction("create", MISP.Arguments.Args(
                MISP.Arguments.Arg("type"), Arguments.Arg("rect"), Arguments.Arg("settings"), Arguments.Optional("hover")),
                "Create a ui element.",
                (context, arguments) =>
                {
                    var type = ScriptObject.AsString(arguments[0]);
                    var rect = arguments[1] as Rectangle?;
                    if (rect == null || !rect.HasValue) throw new InvalidOperationException("Second argument must be a rect.");
                    UIItem result = null;
                    if (type == "item") result = new UIItem(rect.Value);
                    else if (type == "button") result = new Button(rect.Value);
                    else throw new InvalidOperationException("Unknown element type.");
                    result.settings = arguments[2] as ScriptObject;
                    result.hoverSettings = arguments[3] as ScriptObject;
                    return result;
                });
        }

    }
}
