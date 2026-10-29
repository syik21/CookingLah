<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="test.aspx.cs"
    Inherits="CookingLah.Pages.LiveSessionPage.test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <title>SignalR Test</title>

    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 40px;
        }

        .test-box {
            max-width: 600px;
            padding: 24px;
            border: 1px solid #ccc;
            border-radius: 10px;
        }

        .status {
            margin-bottom: 20px;
        }

        .connected {
            color: green;
            font-weight: bold;
        }

        .disconnected {
            color: red;
            font-weight: bold;
        }

        .message-row {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
        }

        .message-row input {
            flex: 1;
            padding: 10px;
        }

        .message-row button {
            padding: 10px 18px;
            cursor: pointer;
        }

        #messageList {
            border: 1px solid #ddd;
            min-height: 150px;
            padding: 15px;
        }

        .message {
            padding: 8px 0;
            border-bottom: 1px solid #eee;
        }
    </style>

</head>

<body>

    <form id="form1" runat="server">

        <div class="test-box">

            <h2>SignalR Test</h2>

            <div class="status">
                Connection:
                <span
                    id="connectionStatus"
                    class="disconnected">
                    Not Connected
                </span>
            </div>

            <div class="status">
                Session:
                <strong>live-session-1</strong>
            </div>

            <div class="status">
                Connection ID:
                <span id="connectionId">
                    -
                </span>
            </div>

            <div class="message-row">

                <input
                    type="text"
                    id="txtMessage"
                    placeholder="Enter a test message" />

                <button
                    type="button"
                    id="btnSend"
                    disabled>
                    Send
                </button>

            </div>

            <h3>Received Messages</h3>

            <div id="messageList">
                No messages yet.
            </div>

        </div>

    </form>


    <!-- jQuery -->
    <script src='<%= ResolveUrl("~/Scripts/jquery-3.7.1.min.js") %>'></script>

    <!-- SignalR client -->
    <script src='<%= ResolveUrl("~/Scripts/jquery.signalR-2.4.3.min.js") %>'></script>

    <!-- Generated SignalR Hub proxy -->
    <script src='<%= ResolveUrl("~/signalr/hubs") %>'></script>


    <script>

        $(function () {

            var liveSessionId = "1";

            var hub =
                $.connection.liveSessionHub;


            // Receive a test message from the server.
            hub.client.receiveTestMessage =
                function (message) {

                    var messageList =
                        $("#messageList");

                    if (
                        messageList.text().trim() ===
                        "No messages yet."
                    ) {
                        messageList.empty();
                    }

                    $("<div>")
                        .addClass("message")
                        .text("Received: " + message)
                        .appendTo(messageList);
                };


            // Enable SignalR debug messages.
            $.connection.hub.logging = true;


            // Start the SignalR connection.
            $.connection.hub.start({
                transport: "longPolling"
            })
                .done(function () {

                    console.log(
                        "SignalR connected."
                    );

                    console.log(
                        "Connection ID:",
                        $.connection.hub.id
                    );

                    console.log(
                        "Transport:",
                        $.connection.hub.transport.name
                    );


                    $("#connectionStatus")
                        .text("Connected")
                        .removeClass("disconnected")
                        .addClass("connected");


                    $("#connectionId")
                        .text(
                            $.connection.hub.id
                        );


                    // Join live-session-1.
                    hub.server
                        .joinLiveSession(
                            liveSessionId
                        )
                        .done(function () {

                            console.log(
                                "Joined live-session-" +
                                liveSessionId
                            );

                            $("#btnSend")
                                .prop(
                                    "disabled",
                                    false
                                );
                        })
                        .fail(function (error) {

                            console.error(
                                "Failed to join group:",
                                error
                            );
                        });

                })
                .fail(function (error) {

                    console.error(
                        "SignalR connection failed:",
                        error
                    );

                    $("#connectionStatus")
                        .text("Connection Failed")
                        .removeClass("connected")
                        .addClass("disconnected");
                });


            // Send a test message.
            $("#btnSend").click(function () {

                var message =
                    $("#txtMessage")
                        .val()
                        .trim();

                if (message === "") {
                    return;
                }


                hub.server
                    .sendTestMessage(
                        liveSessionId,
                        message
                    )
                    .done(function () {

                        $("#txtMessage")
                            .val("");
                    })
                    .fail(function (error) {

                        console.error(
                            "Message failed:",
                            error
                        );
                    });

            });


            // Allow Enter key to send message.
            $("#txtMessage").keypress(
                function (event) {

                    if (event.which === 13) {

                        event.preventDefault();

                        $("#btnSend")
                            .click();
                    }
                }
            );

        });

    </script>

</body>
</html>