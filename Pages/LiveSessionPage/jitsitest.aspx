<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="jitsitest.aspx.cs" Inherits="CookingLah.Pages.LiveSessionPage.jitsitest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Jitsi Test</title>
</head>

<body>

    <form id="form1" runat="server">

        <h2>Cooking LAH Jitsi Test</h2>

        <div id="jitsi-container"></div>

    </form>


    <script src="https://meet.jit.si/external_api.js"></script>


    <script>

const domain =
    "meet.jit.si";

const options = {

    roomName:
        "CookingLahTestRoom001",

    width:
        "100%",

    height:
        600,

    parentNode:
        document.querySelector(
            "#jitsi-container"
        ),

    userInfo: {
        displayName:
            "Test Chef"
    }
};


const api =
    new JitsiMeetExternalAPI(
        domain,
        options
    );

</script>

</body>
</html>