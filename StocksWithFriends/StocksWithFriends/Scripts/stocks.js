var activeSymbol = "";

$(function () {
    $('#fetch').on('click', function () {
        updatePrice($('#tradeSymbol').val());
    });

    $('#executeBuy').on('click', function () {
        console.log("executing buy");
        $.ajax({
            dataType: "json",
            url: 'Stock/BuyStock',
            data: { symbol: $('#tradeSymbol').val(), quantity: $('#quantity').val() }
        }).done(function (data) {
            console.log(data);
            console.log("result: " + data.Item1);
            if (data.Item1) {
                console.log(data.Item2);
            }
            else { console.log("Error: " + data.Item2); }
        });
        console.log("completed?");
    });

    $('#executeSell').on('click', function () {
        console.log("executing sell");
        $.ajax({
            dataType: "json",
            url: 'Stock/SellStock',
            data: { symbol: $('#tradeSymbol').val(), quantity: $('#quantity').val() }
        }).done(function (data) {
            console.log(data);
            console.log("result: " + data.Item1);
            if (data.Item1) {
                console.log(data.Item2);
            }
            else { console.log("Error: " + data.Item2); }
        });
        console.log("completed?");
    });

    $('.save').on('click', function () {
        console.log("saving note for " + activeSymbol);
        console.log("note: " + $('#note-' + activeSymbol).val());
        $.ajax({
            dataType: "json",
            url: 'Stock/SaveNote',
            data: { symbol: activeSymbol, note: $('#note-' + activeSymbol).val() }
        }).done(function (data) {
            console.log(data);
            console.log("result: " + data.Item1);
            if (data.Item1) {
                console.log("Saved note:\n" + data.Item2);
            }
            else { console.log("Error: " + data.Item2); }
        });
    });
});

function makeVisible(symbol) {
    var item = document.getElementById(symbol);
    var contentPanel = document.getElementById("stocks-right-pane");
    var contents = contentPanel.getElementsByTagName("section");

    console.log("making visible: " + symbol);
    for (var i = 0; i < contents.length; i++) {
        if (contents[i] != item) {
            contents[i].style.display = "none"
        }
        else {
            item.style.display = "";
        }
    }

    activeSymbol = symbol.substring(symbol.lastIndexOf('-') + 1);

    $('#tradeSymbol').val(activeSymbol);
    updatePrice(activeSymbol);

    item.style.display = "";
}

function updatePrice(s) {
    $.ajax({
        dataType: "json",
        url: 'Stock/GetStock',
        data: { symbol: s },
    }).done(function (data) {
        if (data.Item1) {
            console.log("returning " + data.Item2.price);
            $('#currentPrice').text('$' + data.Item2.price);
        }
        else {
            console.log("invalid symbol: " + s);
            $('#currentPrice').text(''); // invalid symbol
        }
    });
}