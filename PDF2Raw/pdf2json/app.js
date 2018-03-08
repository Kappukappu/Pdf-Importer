﻿console.log('Loading');
let PDFParser = require("pdf2json");
var stream = require('stream');


exports.myHandler = function (event, context, callback) {
    console.log('Handler executing');
    if (event !== null) {
        console.log('event body found');
        console.log(event);
    }
    else {
        console.log('No event object');

    }

    var pdfParser = new PDFParser();

    const buf = Buffer.from(event, 'base64');

    pdfParser.on("pdfParser_dataError", errData => console.error(errData.parserError));
    pdfParser.on("pdfParser_dataReady", pdfData => {
        console.log(JSON.parse(JSON.stringify(pdfData)));
        callback(null, JSON.parse(JSON.stringify(pdfData)));
    });

    pdfParser.parseBuffer(buf);


    //console.log('data: ' + data);

    //callback(null, 'Hello World');
    //context.done(null, 'Hello World');  // SUCCESS with message
};

/*
JS version parser:
function secondParser(data) {
    //parse the .json output from pdf2json into an array called pureText
    //there are x, y, text, size in each pureText object
    const file = JSON.parse(data)["formImage"];
    const content = file["Pages"][0];
    const text = content["Texts"];
    let pureText = [];
    const length = text.length;
    for (let i = 0; i < length; i++) {
        let temp = {};
        const current = text[i];
        const currentText = current["R"][0];
        temp["x"] = current["x"];
        temp["y"] = current["y"];
        temp["text"] = decodeURI(currentText["T"]);
        temp["text"] = temp["text"].replace(/%2C/g, ",");
        temp["text"] = temp["text"].replace(/%2F/g, "/");
        temp["text"] = temp["text"].replace(/%3A/g, ":");
        temp["size"] = currentText["TS"][1];
        pureText[i] = temp;
    }

    //build table
    let table = {};
    let columnTitles = [];
    var columnCount = 0;
    for (let i = 0; i < length; i++) {
        const item = pureText[i];
        //add column titles
        if ((item["x"] > 5) && (item["y"] < 10)) {
            let newColumnTitle = {};
            newColumnTitle["text"] = item["text"];
            newColumnTitle["x"] = item["x"];

            //see if needed to combine with next line
            let nextItem = pureText[i + 1];
            let currentItem = item;
            while ((nextItem["y"] - currentItem["y"] < 1) && (nextItem["y"] - currentItem["y"] > 0)) {
                newColumnTitle["text"] += nextItem["text"];
                i++;
                currentItem = nextItem;
                nextItem = pureText[i + 1];
            }
            columnTitles[columnCount] = newColumnTitle;
            columnCount++;
        }
        //add a row
        else {
            let newRowItem = {};
            //for title/data/etc that above column titles
            if (columnCount !== 0) {
                var j = 0;
                var keys = 0;
                for (let j = 0; j < columnCount; j++) {
                    const temp = pureText[i + j + 1];
                    if ((temp === undefined) || (temp["y"] - item["y"] > 0.5)) {
                        break;
                    }
                    else {
                        for (let k = j; k < columnCount; k++) {
                            if (Math.abs(temp["x"] - columnTitles[k]["x"]) < 2.6) {
                                const keyName = columnTitles[k]["text"];
                                newRowItem[keyName] = temp["text"];
                                keys++;
                            }
                        }
                    }
                }
                i += keys;

            }
            table[item["text"]] = newRowItem;
        }
    }


    return table;
}
*/