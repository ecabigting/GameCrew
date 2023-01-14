function encodeImgtoBase64(element) {
    var file = element.files[0];
    if (typeof (file) != "undefined") {
        var size = parseFloat(file.size / 1000).toFixed(2);
        if (size > 100) {
            alert('Please select image size less than 100 kb');
        } else {
            var reader = new FileReader();
            reader.onloadend = function () {
                $("#convertImg").attr("href", reader.result);
                $("#Image").val(reader.result.toString());
                $("#ImagePrev").attr("src", reader.result);
            }
            reader.readAsDataURL(file);
        }
    } else {
        alert("This browser does not support HTML5.");
    }
}