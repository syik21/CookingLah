let api = null;
let chefId = null;

// Start the Chef's Jitsi session using the page's session details.
function loadJitsi() {

    const sessionId =
        document.getElementById("hfLiveSessionID")?.value;

    const roomName =
        document.getElementById("hfJitsiRoomName")?.value;

    const chefDisplayName =
        document.getElementById("hfChefDisplayName")?.value;

    const container =
        document.getElementById("jitsi-container");

    if (!sessionId || sessionId === "0" || !roomName || !chefDisplayName || !container) {
        console.error("Invalid live session.");
        return;
    }

    if (!window.jitsiDomain ||
        typeof JitsiMeetExternalAPI === "undefined") {
        console.error("Jitsi API is unavailable.");
        return;
    }

    // Prevent duplicate Jitsi instances.
    if (api) return;

    const options = {
        roomName: roomName,
        width: "100%",
        height: "100%",
        parentNode: container,

        userInfo: {
            displayName: chefDisplayName
        },

        configOverwrite: {
            prejoinConfig: {
                enabled: false
            },

            startWithAudioMuted: false,
            startWithVideoMuted: false,
            disableDeepLinking: true,
            hideConferenceSubject: true,
            hideConferenceTimer: true,
            hideParticipantsStats: true,
            disableSelfView: false,
            doNotFlipLocalVideo: false,

            filmstrip: {
                disabled: true
            },

            notifications: [],
            conferenceInfo: {
                alwaysVisible: [],
                autoHide: []
            },

            toolbarButtons: [
                "microphone",
                "camera"
            ]
        },

        interfaceConfigOverwrite: {
            SHOW_JITSI_WATERMARK: false,
            SHOW_BRAND_WATERMARK: false,
            SHOW_POWERED_BY: false
        }
    };

    api = new JitsiMeetExternalAPI(
        window.jitsiDomain,
        options
    );

    function updateParticipantCount() {
        const count = api.getNumberOfParticipants() - 1;
        document.getElementById("liveParticipantCount").textContent =
            count + (count === 1 ? " person" : " people") + " in this live";
    }

    // When Chef joins the meeting.
    api.addListener("videoConferenceJoined", function (event) {

        chefId = event.id;

        console.log("Chef joined:", chefId);

        // Disable tile layout only once.
        api.executeCommand("setTileView", false);

        // Try to pin Chef's camera only once.
        api.pinParticipant(chefId, "camera");

        // Select Chef as main video only once.
        api.setLargeVideoParticipant(chefId);

        updateParticipantCount();
    });

    // When a new learner joins.
    api.addListener("participantJoined", function (event) {

        console.log("Learner joined:", event.id);

        updateParticipantCount();

        // Do NOT reload Jitsi.
        // Do NOT reselect the Chef's video.
    });

    api.addListener("participantLeft", function (event) {
        console.log("Participant left:", event.id);
        updateParticipantCount();
    });

    api.addListener("participantRoleChanged", function (event) {

        if (event.role === "moderator") {
            console.log("Chef is moderator.");

            // Enable audio moderation.
            api.executeCommand("toggleModeration", true, "audio");

            // Mute other participants.
            api.executeCommand("muteEveryone", "audio");
        }
    });

    api.addListener("errorOccurred", function (error) {
        console.error("Jitsi error:", error);
    });

    api.addListener("videoConferenceLeft", function () {
        console.log("Chef left the meeting.");
    });
}

function reloadJitsi() {

    if (api) {
        api.dispose();
        api = null;
    }

    chefId = null;
    loadJitsi();
}

document.addEventListener("DOMContentLoaded", loadJitsi);

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("btnAcknowledgeWarning").addEventListener("click", function () {
        document.getElementById("warningDialog").close();
    });
});

window.addEventListener("cookinglah:admin-warning", function (event) {
    document.getElementById("lblWarningCount").textContent = event.detail.warningNumber;
    document.getElementById("warningTitle").textContent =
        "Admin Warning " + event.detail.warningNumber;
    document.getElementById("warningReason").textContent = event.detail.reason;
    const dialog = document.getElementById("warningDialog");
    if (!dialog.open) {
        dialog.showModal();
    }
});

window.addEventListener("cookinglah:live-suspended", function (event) {
    const warningDialog = document.getElementById("warningDialog");
    if (warningDialog.open) {
        warningDialog.close();
    }

    if (api) {
        api.dispose();
        api = null;
    }

    const container = document.getElementById("jitsi-container");
    container.innerHTML = "";
    const message = document.createElement("div");
    message.className = "live-ended-message";
    message.innerHTML = "<h2>Livestream suspended</h2><p></p>";
    message.querySelector("p").textContent = event.detail.reason;
    container.appendChild(message);
    document.getElementById("txtChatMessage").disabled = true;
    document.getElementById("btnSendMessage").disabled = true;
    document.getElementById("liveParticipantCount").textContent = "Suspended";
    $.connection.hub.stop();
});
