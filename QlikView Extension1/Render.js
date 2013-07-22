// Rendering code example
while (this.Element.firstChild) this.Element.removeChild(this.Element.firstChild);
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
