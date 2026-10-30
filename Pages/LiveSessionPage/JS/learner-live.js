
let api = null;
let chefParticipantId = null;
let resizeObserver = null;

window.addEventListener("cookinglah:live-ended", function () {
    console.log("Chef ended the live class.");

    if (api) {
        api.dispose();
        api = null;
    }

    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    const container = document.getElementById("jitsi-container");
    const endedMessage = document.createElement("div");
    const title = document.createElement("h2");
    const detail = document.createElement("p");

    endedMessage.className = "live-ended-message";
    endedMessage.setAttribute("role", "status");
    title.textContent = "This live has ended";
    detail.textContent = "Thank you for joining the class.";
    endedMessage.append(title, detail);
    container.replaceChildren(endedMessage);

    document.getElementById("txtChatMessage").disabled = true;
    document.getElementById("btnSendMessage").disabled = true;
    document.getElementById("liveParticipantCount").textContent = "Live ended";

    $.connection.hub.stop();
});

function loadJitsi() {

    const sessionId =
        document.getElementById("hfLiveSessionID")?.value;

    const container =
        document.getElementById("jitsi-container");

    if (!sessionId || sessionId === "0" || !container) {
        console.error("Invalid session ID or Jitsi container.");
        return;
    }

    if (!window.jitsiDomain ||
        typeof JitsiMeetExternalAPI === "undefined") {
        console.error("Jitsi API is unavailable.");
        return;
    }

    // Prevent duplicate meetings.
    if (api) return;

    const options = {
        roomName: "LiveClass_" + sessionId,
        width: "100%",
        height: "100%",
        parentNode: container,

        userInfo: {
            displayName: "Learner"
        },

        configOverwrite: {
            prejoinConfig: {
                enabled: false
            },

            startWithAudioMuted: true,
            startWithVideoMuted: true,
            disableSelfView: true,
            doNotFlipLocalVideo: true,

            hideDisplayName: true,
            hideDominantSpeakerBadge: true,

            filmstrip: {
                disabled: true
            },

            tileView: {
                disabled: true
            },

            notifications: [],
            toolbarButtons: []
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

    function resizeJitsi() {
        if (api && container.clientWidth && container.clientHeight) {
            api.resizeLargeVideo(container.clientWidth, container.clientHeight);
        }
    }

    function updateParticipantCount() {
        const count = api.getNumberOfParticipants();
        document.getElementById("liveParticipantCount").textContent =
            count + (count === 1 ? " person" : " people") + " in this live";
    }

    resizeObserver = new ResizeObserver(resizeJitsi);
    resizeObserver.observe(container);

    // Find and pin the Chef only when necessary.
    async function showChefVideo() {

        // Already pinned. Do not change the video again.
        if (!api || chefParticipantId) return;

        try {
            const rooms = await api.getRoomsInfo();

            const mainRoom = rooms.find(
                room => room.isMainRoom
            );

            if (!mainRoom || !mainRoom.participants) return;

            const chef = mainRoom.participants.find(
                participant => participant.displayName === "Test Chef"
            );

            if (!chef) {
                console.log("Chef has not joined yet.");
                return;
            }

            // Save the Chef's Jitsi participant ID.
            chefParticipantId = chef.id;

            // Disable tile view.
            api.executeCommand("setTileView", false);

            // Pin the Chef's camera.
            api.pinParticipant(chefParticipantId, "camera");

            // Select Chef as the large video once.
            api.setLargeVideoParticipant(chefParticipantId);

            resizeJitsi();

            console.log("Chef pinned:", chefParticipantId);

        } catch (error) {
            console.error("Unable to pin Chef:", error);
        }
    }

    // When learner joins the meeting.
    api.addListener("videoConferenceJoined", function () {

        console.log("Learner joined Jitsi.");

        requestAnimationFrame(resizeJitsi);
        updateParticipantCount();
        setTimeout(showChefVideo, 500);
    });

    // When another participant joins.
    api.addListener("participantJoined", function (event) {

        console.log("Participant joined:", event.id);
        updateParticipantCount();

        // Only search if the Chef has not been pinned.
        if (!chefParticipantId) {
            showChefVideo();
        }
    });

    // When a participant leaves.
    api.addListener("participantLeft", function (event) {

        console.log("Participant left:", event.id);
        updateParticipantCount();

        // Only update if the Chef leaves.
        if (event.id === chefParticipantId) {

            chefParticipantId = null;

            console.log("Chef left the meeting.");

            // Do not immediately switch to another learner.
            // Wait for the Chef to rejoin.
        }
    });

    api.addListener("errorOccurred", function (error) {
        console.error("Jitsi error:", error);
    });

    api.addListener("videoConferenceLeft", function () {
        console.log("Learner left the meeting.");
    });
}

// Reload only when manually requested.
function reloadJitsi() {

    if (api) {
        api.dispose();
        api = null;
    }

    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    chefParticipantId = null;

    loadJitsi();
}

document.addEventListener("DOMContentLoaded", loadJitsi);
