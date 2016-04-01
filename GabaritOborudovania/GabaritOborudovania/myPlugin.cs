// (C) Copyright 2016 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(GabaritOborudovania.MyPlugin))]

namespace GabaritOborudovania
{

   
    public class MyPlugin : IExtensionApplication
    {

        void IExtensionApplication.Initialize()
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("Габариты оборудования инициализированы.." + Environment.NewLine);
        }

        void IExtensionApplication.Terminate()
        {
            
        }

    }

}
