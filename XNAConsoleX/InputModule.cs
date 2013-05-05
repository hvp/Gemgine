using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISP;
using System.Windows.Forms;

namespace XNAConsole
{
    public class InputModule
    {
        private Dictionary<UInt32, ScriptObject> clickBindings = new Dictionary<uint, ScriptObject>();
        private Main main;

        public InputModule(Main main) { this.main = main; }

        public void HandleInput(MouseInput Input)
        {
            if (Input.MousePressed())
                if (clickBindings.ContainsKey(Input.MouseObject))
                    main.events.Add(clickBindings[Input.MouseObject]);
        }


        public void BindScript(Engine scriptEngine)
        {
            scriptEngine.AddFunction("clear-click-events", "Clear all click events.",
                (context, arguments) =>
                {
                    clickBindings.Clear();
                    return null;
                });
            
            scriptEngine.AddFunction("on-click", "Create an on-click event handler for an entity.",
                (context, arguments) =>
                    {
                        var objectID = AutoBind.UIntArgument(arguments[0]);
                        var body = arguments[1];
                        var lambda = Function.MakeFunction("on-click lambda", new ScriptList(), "",
                            body as ScriptObject, context.Scope, true);
                        clickBindings.Upsert(objectID, lambda);
                        return true;
                    },
                Arguments.Arg("object-id", "id of the clicked object."),
                Arguments.Lazy("code"));
        }
    }
}
