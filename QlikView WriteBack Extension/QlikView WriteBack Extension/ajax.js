function writeBack(but) {

    //alert($('select#dateSelect').val());
    //alert($('input#hidden').val());

    $.ajax({
        url: "http://www.webservicex.net/CurrencyConvertor.asmx?op=ConversionRate",
        type: "POST",
        dataType: "xml",
        data:
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                    "<ConversionRate xmlns=\"http://www.webserviceX.NET/\">" +
                        "<FromCurrency>USD</FromCurrency>" +
                        "<ToCurrency>EUR</ToCurrency>" +
                    "</ConversionRate>" +
                "</soap:Body>" +
            "</soap:Envelope>",
        contentType: "text/xml; charset=\"utf-8\"",
        complete: OnSuccess,
        error: OnError
    });
}

 



function OnSuccess(xmlHttpRequest, status)
{
    $(xmlHttpRequest.responseXML)
        .find('ConversionRateResult')
        .each(function()
            {
            var name = $(this).text();
            alert(name);
            });
}

function OnError(request, status, error) {
    alert('error');
    alert(error);
}

$(document).ready(function () {
    $.support.cors = true;
});