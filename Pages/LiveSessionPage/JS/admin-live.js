(function () {
    "use strict";

    if (window.cookingLahAdminLiveLoaded) return;
    window.cookingLahAdminLiveLoaded = true;

    var api = null;
    var chefParticipantId = null;
    var chefSearchTimer = null;
    var adminJoinReported = false;
    var sessionClosed = false;
    var sessionId = Number(document.getElementById("hfLiveSessionID").value);

    window.addEventListener("cookinglah:chat-joined", reportAdminJoined);
    window.addEventListener("cookinglah:admin-warning", showWarningStatus);
    window.addEventListener("cookinglah:chef-presence", updateChefPresence);
    window.addEventListener("cookinglah:live-ended", showEndedState);
    window.addEventListener("cookinglah:live-suspended", showSuspendedState);
    window.addEventListener("pagehide", cleanup);
    document.addEventListener("DOMContentLoaded", initialize);

    function initialize() {
        initializeLiveStream();
        initializeModals();
    }

    // Join the Jitsi room as a muted administrator.
    function initializeLiveStream() {
        var container = document.getElementById("jitsi-container");
        var roomName = document.getElementById("hfJitsiRoomName").value;
        var domain = document.getElementById("hfJitsiDomain").value;
        var adminName = document.getElementById("hfAdminDisplayName").value;

        if (!sessionId || !container || !roomName || !domain || typeof JitsiMeetExternalAPI === "undefined") {
            showStatus("Unable to connect to this livestream.", "error");
            return;
        }

        api = new JitsiMeetExternalAPI(domain, {
            roomName: roomName,
            width: "100%",
            height: "100%",
            parentNode: container,
            userInfo: { displayName: adminName + " (Admin)" },
            configOverwrite: {
                prejoinConfig: { enabled: false },
                startWithAudioMuted: true,
                startWithVideoMuted: true,
                disableSelfView: true,
                disableDeepLinking: true,
                hideConferenceSubject: true,
                hideConferenceTimer: true,
                hideParticipantsStats: true,
                filmstrip: { disabled: true },
                tileView: { disabled: true },
                notifications: [],
                conferenceInfo: {
                    alwaysVisible: [],
                    autoHide: []
                },
                toolbarButtons: []
            },
            interfaceConfigOverwrite: {
                SHOW_JITSI_WATERMARK: false,
                SHOW_BRAND_WATERMARK: false,
                SHOW_POWERED_BY: false
            }
        });

        api.addListener("videoConferenceJoined", function () {
            updateParticipantCount();
            findAndPinChef(10);
        });

        api.addListener("participantJoined", function () {
            updateParticipantCount();
            if (!chefParticipantId) findAndPinChef(10);
        });

        api.addListener("participantLeft", function (event) {
            updateParticipantCount();
            if (event.id === chefParticipantId) {
                chefParticipantId = null;
                clearTimeout(chefSearchTimer);
            }
        });

        api.addListener("errorOccurred", function (error) {
            console.error("Admin Jitsi error:", error);
        });
    }

    async function findAndPinChef(attemptsRemaining) {
        if (!api || chefParticipantId) return;

        try {
            var rooms = await api.getRoomsInfo();
            var mainRoom = rooms.find(function (room) { return room.isMainRoom; });
            var chefName = document.getElementById("hfChefDisplayName").value;
            var chef = mainRoom && mainRoom.participants && mainRoom.participants.find(function (participant) {
                return participant.displayName === chefName;
            });

            if (!chef) {
                scheduleChefSearch(attemptsRemaining);
                return;
            }

            chefParticipantId = chef.id;
            clearTimeout(chefSearchTimer);
            api.executeCommand("setTileView", false);
            api.pinParticipant(chefParticipantId, "camera");
            api.setLargeVideoParticipant(chefParticipantId);
        } catch (error) {
            console.error("Unable to pin Chef video:", error);
            scheduleChefSearch(attemptsRemaining);
        }
    }

    function scheduleChefSearch(attemptsRemaining) {
        clearTimeout(chefSearchTimer);
        if (attemptsRemaining <= 0) return;

        chefSearchTimer = setTimeout(function () {
            findAndPinChef(attemptsRemaining - 1);
        }, 500);
    }

    function updateParticipantCount() {
        if (!api) return;
        var count = Math.max(api.getNumberOfParticipants() - 1, 0);
        document.getElementById("liveParticipantCount").textContent =
            count + (count === 1 ? " person" : " people") + " in this live";
    }

    // Announce one actual Admin page connection.
    function reportAdminJoined() {
        if (adminJoinReported || !window.liveHub) return;
        adminJoinReported = true;
        window.liveHub.server.reportAdminJoined(sessionId).fail(function (error) {
            adminJoinReported = false;
            showStatus(readError(error), "error");
        });
    }

    function initializeModals() {
        bindOpenButton("btnOpenWarning", "warningDialog", "warningReason");
        bindOpenButton("btnOpenSuspension", "suspensionDialog", "suspensionReason");

        document.querySelectorAll("[data-close-dialog]").forEach(function (button) {
            button.addEventListener("click", function () {
                document.getElementById(button.getAttribute("data-close-dialog")).close();
            });
        });

        document.getElementById("btnSubmitWarning").addEventListener("click", submitWarning);
        document.getElementById("btnSubmitSuspension").addEventListener("click", submitSuspension);
    }

    function bindOpenButton(buttonId, dialogId, inputId) {
        document.getElementById(buttonId).addEventListener("click", function () {
            var dialog = document.getElementById(dialogId);
            dialog.showModal();
            document.getElementById(inputId).focus();
        });
    }

    function submitWarning() {
        submitModeration({
            buttonId: "btnSubmitWarning",
            inputId: "warningReason",
            errorId: "warningError",
            action: function (reason) { return window.liveHub.server.issueWarning(sessionId, reason); },
            success: function () {
                document.getElementById("warningDialog").close();
                document.getElementById("warningReason").value = "";
                showStatus("Warning sent to the Chef.", "success");
            }
        });
    }

    function submitSuspension() {
        submitModeration({
            buttonId: "btnSubmitSuspension",
            inputId: "suspensionReason",
            errorId: "suspensionError",
            action: function (reason) { return window.liveHub.server.suspendLive(sessionId, reason); },
            success: function () {
                document.getElementById("suspensionDialog").close();
            }
        });
    }

    function submitModeration(options) {
        var button = document.getElementById(options.buttonId);
        var input = document.getElementById(options.inputId);
        var error = document.getElementById(options.errorId);
        var reason = input.value.trim();

        error.hidden = true;
        if (!reason) {
            error.textContent = "Enter a reason before continuing.";
            error.hidden = false;
            input.focus();
            return;
        }

        button.disabled = true;
        options.action(reason).done(options.success).fail(function (requestError) {
            error.textContent = readError(requestError);
            error.hidden = false;
        }).always(function () {
            button.disabled = false;
        });
    }

    function showWarningStatus(event) {
        document.getElementById("lblWarningCount").textContent = event.detail.warningNumber;
        showStatus("Warning " + event.detail.warningNumber + " issued: " + event.detail.reason, "warning");
    }

    function updateChefPresence(event) {
        if (sessionClosed) return;

        var message = document.getElementById("chefPausedMessage");
        if (event.detail.present) {
            if (message) message.hidden = true;
            return;
        }

        var container = document.getElementById("jitsi-container");
        if (!container) return;

        if (!message) {
            message = document.createElement("div");
            message.id = "chefPausedMessage";
            message.className = "chef-paused-message";
            message.setAttribute("role", "status");
            message.innerHTML = "<h2>Stream paused</h2><p>The Chef paused the stream for a while.</p>";
            container.appendChild(message);
        }

        message.hidden = false;
    }

    function showEndedState() {
        sessionClosed = true;
        cleanup();
        document.querySelectorAll(".admin-action").forEach(function (button) { button.disabled = true; });
        document.querySelectorAll(".admin-moderation-dialog[open]").forEach(function (dialog) { dialog.close(); });

        var container = document.getElementById("jitsi-container");
        container.innerHTML = "";
        var message = document.createElement("div");
        message.className = "live-ended-message";
        message.innerHTML = "<h2>This live has ended</h2><p>The Chef ended the live session.</p>";
        container.appendChild(message);

        document.getElementById("txtChatMessage").disabled = true;
        document.getElementById("btnSendMessage").disabled = true;
        document.getElementById("liveParticipantCount").textContent = "Live ended";
        $.connection.hub.stop();
        showStatus("The Chef ended the livestream.", "warning");
    }

    function showSuspendedState(event) {
        sessionClosed = true;
        cleanup();
        document.querySelectorAll(".admin-action").forEach(function (button) { button.disabled = true; });
        document.querySelectorAll(".admin-moderation-dialog[open]").forEach(function (dialog) { dialog.close(); });
        document.getElementById("txtChatMessage").disabled = true;
        document.getElementById("btnSendMessage").disabled = true;

        var container = document.getElementById("jitsi-container");
        container.innerHTML = "";
        var message = document.createElement("div");
        message.className = "live-ended-message";
        message.innerHTML = "<h2>Livestream suspended</h2><p></p>";
        message.querySelector("p").textContent = event.detail.reason;
        container.appendChild(message);
        showStatus("The livestream was suspended successfully.", "success");
    }

    function showStatus(message, type) {
        var status = document.getElementById("adminModerationStatus");
        status.textContent = message;
        status.className = "admin-moderation-status admin-moderation-status--" + type;
        status.hidden = false;
    }

    window.exitAdminLive = function (button) {
        if (!window.confirm("Are you sure you want to exit this live session?")) return false;

        clearTimeout(chefSearchTimer);
        button.disabled = true;

        var finishExit = function () {
            __doPostBack(button.name, "");
        };

        leaveJitsi(function () {
            if (window.leaveLiveChat) {
                window.leaveLiveChat().always(finishExit);
            } else {
                finishExit();
            }
        });

        return false;
    };

    function leaveJitsi(done) {
        var currentApi = api;
        api = null;

        if (!currentApi) {
            done();
            return;
        }

        var completed = false;
        var finish = function () {
            if (completed) return;
            completed = true;
            currentApi.dispose();
            done();
        };

        currentApi.addListener("videoConferenceLeft", finish);
        currentApi.executeCommand("hangup");
        setTimeout(finish, 1500);
    }

    function readError(error) {
        return error && error.message ? error.message : "The moderation action could not be completed.";
    }

    function cleanup() {
        clearTimeout(chefSearchTimer);
        if (api) {
            api.dispose();
            api = null;
        }
    }
}());
