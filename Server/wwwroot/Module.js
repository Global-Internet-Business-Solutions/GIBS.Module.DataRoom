/* Module Script */
var GIBS = GIBS || {};

GIBS.DataRoom = GIBS.DataRoom || {};

GIBS.DataRoom.getCultureFromCookie = function() {
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return decodeURIComponent(parts.pop().split(';').shift());
        return null;
    }
    const result = getCookie('.AspNetCore.Culture') || '';
    console.log('getCultureFromCookie result:', result);
    return result;
};

GIBS.DataRoom.setCultureCookie = function(cultureValue) {
    // Set the .AspNetCore.Culture cookie with the specified value
    // The value format should be "c=it|uic=en"
    const expires = new Date();
    expires.setTime(expires.getTime() + (365 * 24 * 60 * 60 * 1000)); // 1 year
    const expiresString = expires.toUTCString();

    document.cookie = `.AspNetCore.Culture=${cultureValue}; expires=${expiresString}; path=/`;
    console.log('setCultureCookie: set to', cultureValue);
};

GIBS.DataRoom.loadPdfIntoFrame = async function (frameId, url, fallbackUrl) {
    try {
        const frame = document.getElementById(frameId);
        if (!frame) {
            return "Viewer frame not found.";
        }

        const fetchPdf = async function (targetUrl) {
            if (!targetUrl) {
                return { ok: false, status: 0, statusText: "No URL" };
            }
            return await fetch(targetUrl, {
                method: "GET",
                credentials: "same-origin"
            });
        };

        let response = await fetchPdf(url);
        if (!response.ok && response.status === 404 && fallbackUrl) {
            response = await fetchPdf(fallbackUrl);
        }

        if (!response.ok) {
            return "Server returned " + response.status + " " + response.statusText;
        }

        const blob = await response.blob();
        const blobUrl = URL.createObjectURL(blob);
        frame.src = blobUrl;
        return "";
    }
    catch (err) {
        return err && err.message ? err.message : "Failed to load PDF.";
    }
};