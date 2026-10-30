$(function () {
    window.liveHub = $.connection.liveSessionHub;
    var hub = window.liveHub;
    var sessionId = document.getElementById("hfLiveSessionID")?.value;
    if (!sessionId) {
        console.error("Live session ID is missing.");
        return;
    }

    // Receive new chat message
    hub.client.receiveChatMessage = function (
        username,
        message,
        createdAt
    ) {
        var chatMessage = $("<div>")
            .addClass("chat-message");

        var info = $("<div>")
            .addClass("chat-message-info");

        $("<strong>")
            .text(username)
            .appendTo(info);

        $("<span>")
            .text(new Date(createdAt).toLocaleTimeString([], {
                hour: "numeric",
                minute: "2-digit"
            }))
            .appendTo(info);

        info.appendTo(chatMessage);

        $("<p>")
            .text(message)
            .appendTo(chatMessage);

        $("#messageList").append(chatMessage);
    };

    hub.client.liveEnded = function (endedSessionId) {
        if (Number(endedSessionId) === Number(sessionId)) {
            window.dispatchEvent(new Event("cookinglah:live-ended"));
        }
    };

    // Disable sending before connection
    $("#btnSendMessage").prop("disabled", true);

    // Start SignalR connection
    $.connection.hub.start().done(function () {

        hub.server.joinChat(Number(sessionId)).done(function () {
            $("#btnSendMessage").prop("disabled", false);
        }).fail(function (error) {
            console.error("Unable to join live chat:", error);
        });

    }).fail(function (error) {
        console.error("SignalR connection failed:", error);
    });

    // Send message
    $("#btnSendMessage").on("click", function () {
        var message = $("#txtChatMessage").val().trim();
        if (!message || $("#btnSendMessage").prop("disabled")) {
            return;
        }
        hub.server.sendChatMessage(Number(sessionId), message)
            .done(function () {
                $("#txtChatMessage").val("");
            })
            .fail(function (error) {
                console.error("Send failed:", error);
            });
    });

    // Use Enter to send
    $("#txtChatMessage").keypress(
        function (event) {
            if (event.which === 13) {
                event.preventDefault();
                $("#btnSendMessage").click();
            }
        }
    );

});
