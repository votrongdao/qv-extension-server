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
