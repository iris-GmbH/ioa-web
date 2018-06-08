/* Should be initialized very early in js pipeline (even before jquery)! */
"use strict";
(function () {
    function id(v) { return document.getElementById(v); }
    function loadbar() {
        var ovrl = id("overlay"),
            prog = id("progress"),
            stat = id("progstat"),
            img = document.images,
            c = 0;
        colCount = img.length;

        function imgLoaded() {
            c += 1;
            var perc = ((100 / colCount * c) << 0) + "%";
            prog.style.width = perc;
            stat.innerHTML = perc;
            if (c === colCount) return doneLoading();
        }
        function doneLoading() {
            ovrl.style.opacity = 0;
            setTimeout(function () {
                ovrl.style.display = "none";
            }, 1200);
        }
        for (var i = 0; i < colCount; i++) {
            var tImg = new Image();
            tImg.onload = imgLoaded;
            tImg.onerror = imgLoaded;
            tImg.src = img[i].src;
        }
    }
    document.addEventListener('DOMContentLoaded', loadbar, false);
}());