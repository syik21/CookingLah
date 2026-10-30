$(function () {
    window.liveHub = $.connection.liveSessionHub;
    var hub = window.liveHub;
    var sessionId = document.getElementById("hfLiveSessionID")?.value;
    var isChefPage = Boolean(document.getElementById("hfChefID"));
    if (!sessionId) {
        console.error("Live session ID is missing.");
        return;
    }

    // Receive new chat message
    hub.client.receiveChatMessage = function (
        username,
        message,
        createdAt,
        role
    ) {
        var chatMessage = $("<div>")
            .addClass("chat-message");

        var info = $("<div>")
            .addClass("chat-message-info");

        $("<strong>")
            .text(username)
            .appendTo(info);

        if (role === "Admin") {
            $("<span>")
                .addClass("chat-role-badge")
                .text("ADMIN")
                .appendTo(info);
        }

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

    hub.client.systemMessage = function (message, type) {
        appendSystemMessage(message, type);
    };

    hub.client.adminWarning = function (reason, warningNumber) {
        appendSystemMessage("Warning " + warningNumber + " issued: " + reason, "warning");
        window.dispatchEvent(new CustomEvent("cookinglah:admin-warning", {
            detail: { reason: reason, warningNumber: Number(warningNumber) }
        }));
    };

    hub.client.liveSuspended = function (reason) {
        appendSystemMessage("This livestream was suspended: " + reason, "suspended");
        window.dispatchEvent(new CustomEvent("cookinglah:live-suspended", {
            detail: { reason: reason }
        }));
    };

    hub.client.liveEnded = function (endedSessionId) {
        if (Number(endedSessionId) === Number(sessionId)) {
            window.dispatchEvent(new Event("cookinglah:live-ended"));
        }
    };

    hub.client.streamPresence = function (chefIsPresent) {
        window.dispatchEvent(new CustomEvent("cookinglah:chef-presence", {
            detail: { present: Boolean(chefIsPresent) }
        }));
    };

    // Disable sending before connection
    $("#btnSendMessage").prop("disabled", true);

    // Start SignalR connection
    $.connection.hub.start().done(function () {

        hub.server.joinChat(Number(sessionId)).done(function () {
            $("#btnSendMessage").prop("disabled", false);

            reportChefPresence();
            window.dispatchEvent(new Event("cookinglah:chat-joined"));
        }).fail(function (error) {
            console.error("Unable to join live chat:", error);
        });

    }).fail(function (error) {
        console.error("SignalR connection failed:", error);
    });

    $.connection.hub.reconnected(reportChefPresence);

    window.leaveLiveChat = function () {
        if ($.connection.hub.state === $.signalR.connectionState.disconnected) {
            return $.Deferred().resolve().promise();
        }

        return hub.server.leaveChat(Number(sessionId)).always(function () {
            $.connection.hub.stop(false, true);
        });
    };

    window.addEventListener("pagehide", function () {
        if (isChefPage && $.connection.hub.state !== $.signalR.connectionState.disconnected) {
            // Tell the server now instead of waiting for the disconnect timeout.
            $.connection.hub.stop(false, true);
        }
    });

    function reportChefPresence() {
        if (!isChefPage) return;

        hub.server.reportChefPresent(Number(sessionId)).fail(function (error) {
            console.error("Unable to report Chef presence:", error);
        });
    }

    function appendSystemMessage(message, type) {
        var item = $("<div>")
            .addClass("chat-message chat-system-message")
            .addClass("chat-system-message--" + (type || "info"));

        $("<p>").text(message).appendTo(item);
        $("#messageList").append(item);
    }

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
