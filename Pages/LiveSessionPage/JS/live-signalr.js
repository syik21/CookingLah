let liveSessionHub = null;

function connectToLiveSession(liveSessionId) {
    liveSessionHub = $.connection.liveSessionHub;

    liveSessionHub.client.receiveWarning = function (
        warningNumber,
        reason
    ) {
        console.log(
            "Warning " + warningNumber + ": " + reason
        );
    };

    liveSessionHub.client.streamClosed = function (reason) {
        console.log("Stream closed: " + reason);

        handleStreamClosed(reason);
    };

    $.connection.hub.start()
        .done(function () {
            console.log("SignalR connected.");

            liveSessionHub.server
                .joinLiveSession(liveSessionId);
        })
        .fail(function () {
            console.log("SignalR connection failed.");
        });
}