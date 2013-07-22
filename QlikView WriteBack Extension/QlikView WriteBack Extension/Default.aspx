<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="QlikView_WriteBack_Extension._Default" %>
<%@ Register assembly="QlikViewExtension" namespace="QT.QWW.WebControls" tagprefix="qww" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <!------------------------------------------ 
      Go to Design view to manage the extension
    -------------------------------------------->
    <qww:QvExtension ID="QvExtension1" runat="server" Name="QlikViewWriteBackExtension" Description="Extension that performs writeback to a SOAP webservice" Type="Chart">
        <Properties>
            <qww:Property Label="First Row" Type="Dimension" />
            <qww:Property Label="Second Row" Type="Dimension" />
            <qww:Property Label="Expression" Type="Expression" />
        </Properties>
        <Scripts>
            <qww:Script Source="ajax.js" />
        </Scripts>
    </qww:QvExtension>

    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
