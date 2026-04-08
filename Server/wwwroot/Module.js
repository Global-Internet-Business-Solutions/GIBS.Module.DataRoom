/* Module Script */
var GIBS = GIBS || {};

GIBS.DataRoom = GIBS.DataRoom || {};

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