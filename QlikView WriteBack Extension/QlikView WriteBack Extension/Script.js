 
function QlikViewWriteBackExtension_Done() {
    Qv.AddExtension("QlikViewWriteBackExtension",
        function() {
            // code written in Render.js will be inserted here
            // Rendering code example
             
             
             
             
             
             while (this.Element.firstChild) this.Element.removeChild(this.Element.firstChild);
             if (this.Data.Rows.length > 10) {
                 var par = document.createElement("p");
                 par.innerHTML = "You need to select less than 10 values in order to bulk update ...";
                 this.Element.appendChild(par);
             } else {
                 var par = document.createElement("p");
             
                 var str = "";
                 for (var r = 0; r < this.Data.Rows.length; r++) {
                     str += this.Data.Rows[r][2].text + ';';
                 }
             
             
                 par.innerHTML = "<select id=\"dateSelect\"><option value=\"1\">1</option><option value=\"2\">2</option></select>" +
                                 "<input id=\"hidden\" type=\"hidden\" value=\"" + str + "\">" +
                                 "<input id=\"refresh\" type=\"button\" value=\"Write Back\" onclick=\"writeBack(this);\" />";
             
                 this.Element.appendChild(par);
             }
             
                
             
             /*
             
             
             var mytable = document.createElement("table");
             mytable.style.width = "100%";
             var noCols = this.Data.HeaderRows[0].length;
             var tablebody = document.createElement("tbody");
             for (var r = 0; r < this.Data.HeaderRows.length; r++) {
                 var row = document.createElement("tr");
                 for (var c = 0; c < noCols; c++) {
                     var cell = document.createElement("td");
                     cell.innerHTML = this.Data.HeaderRows[r][c].text;
                     row.appendChild(cell);
                 }
                 tablebody.appendChild(row);
             }
             for (var r = 0; r < this.Data.Rows.length; r++) {
                 var row = document.createElement("tr");
                 for (var c = 0; c < noCols; c++) {
                     var cell = document.createElement("td");
                     cell.innerHTML = this.Data.Rows[r][c].text;
                     row.appendChild(cell);
                 }
                 tablebody.appendChild(row);
             }
             mytable.appendChild(tablebody);
             this.Element.appendChild(mytable);
             */
             }, false);
}
function QlikViewWriteBackExtension_Init() {
    var files = [];
    files.push("Extensions/QlikViewWriteBackExtension/ajax.js");
    Qv.LoadExtensionScripts(files, QlikViewWriteBackExtension_Done);
}
QlikViewWriteBackExtension_Init();

