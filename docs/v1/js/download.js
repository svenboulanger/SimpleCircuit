function DownloadData(filename, exportUrl) {
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_self";
    a.click();
    document.body.removeChild(a);
}

// Source code copied and modified from Gérald Barré
function BlazorDownloadFile(filename, contentType, data) {
    const file = new File([data], filename, { type: contentType });
    const exportUrl = URL.createObjectURL(file);
    DownloadData(filename, exportUrl);
    URL.revokeObjectURL(exportUrl);
}

function BlazorExportImage(filename, mime, imageData, width, height, backgroundColor) {
    var image = new Image();
    image.onload = function () {
        var canvas = document.getElementById("canvas");
        canvas.width = width;
        canvas.height = height;
        var context = canvas.getContext("2d");
        if (backgroundColor) {
            context.fillStyle = backgroundColor;
            context.fillRect(0, 0, width, height);
        }
        context.drawImage(image, 0, 0, width, height);
        DownloadData(filename, canvas.toDataURL(mime));
    };
    image.src = imageData;
}