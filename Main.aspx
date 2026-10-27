 <%@ Page Title="Cooking LAH | Cook your way around the world" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="CookingLah.Main" %>
<%@ Register Src="~/Pages/Shared/WorldMap/WorldMap.ascx" TagPrefix="uc" TagName="WorldMap" %>
<asp:Content ID="MapNavigation" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        document.addEventListener('worldmap:countrychange', function (event) {
            window.location.href = '<%= ResolveUrl("~/Pages/Guest/Map.aspx") %>?country=' + encodeURIComponent(event.detail.country);
        });
    </script>
</asp:Content>
<asp:Content ID="MainHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/F_CSS/Main.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="HomeContent" ContentPlaceHolderID="MainContent" runat="server">

            <section class="main-hero">
               <div class="container">
                    <h1>Cook your way around <mark>the whole world, lah.</mark></h1>
                    <p class="main-hero__text">Pick any country on the map and see what its kitchen is about, then cook it at home.</p>
                    <img runat="server" class="main-hero__img" src="~/F_Assets/tools.png" alt="" />
                    <img runat="server" class="main-hero__img2" src="~/F_Assets/bread.png" alt="" />
                </div>

                <div class="main-stickers" aria-hidden="true">
                    <div class="sticker main-sticker-a">What do people eat for breakfast in Malaysia?</div>
                    <div class="sticker sticker-ink main-sticker-b">Give me a 20 minute dinner from Thailand</div>
                    <div class="sticker main-sticker-c">Which spices make Moroccan food taste warm?</div>
                    <div class="sticker sticker-primary main-sticker-d">Easy street food I can fake at home</div>
                </div>
            </section>

            <section class="main-food-map" aria-label="Explore food around the world"><uc:WorldMap ID="WorldMapControl" runat="server" /></section>

           <section class="container recipes-section" id="recipes">
                <h2 class="section-title">Cooking basics: start with these</h2>
                <p class="section-description">Tested, short and written for a normal kitchen.</p>
                <a class="recipe-row" href="#">
                    <span class="recipe-frame"><img runat="server" src="~/F_Assets/Food/nasi lemak.png" alt="Nasi lemak" /></span>
                    <div><h3 class="recipe-row__title">Nasi lemak, the proper way</h3><p class="recipe-row__desc">Malaysia. Coconut rice, sambal and crispy anchovies.</p></div>
                    <div class="recipe-row__meta"><span class="recipe-row__time">30m</span><span>Easy</span></div>
                </a>
                <a class="recipe-row" href="#">
                    <span class="recipe-frame"><img runat="server" src="~/F_Assets/padkrapao.png" alt="Pad kra pao" /></span>
                    <div><h3 class="recipe-row__title">Pad kra pao for one</h3><p class="recipe-row__desc">Thailand. Holy basil, chilli and a fried egg on top.</p></div>
                    <div class="recipe-row__meta"><span class="recipe-row__time">25m</span><span>Easy</span></div>
                </a>
                <a class="recipe-row" href="#">
                    <span class="recipe-frame"><img runat="server" src="~/F_Assets/tacos.png" alt="Tacos al pastor" /></span>
                    <div><h3 class="recipe-row__title">Tacos al pastor without a spit</h3><p class="recipe-row__desc">Mexico. Pork, pineapple and a hot pan.</p></div>
                    <div class="recipe-row__meta"><span class="recipe-row__time">45m</span><span>Medium</span></div>
                </a>
            </section>

            <section class="container main-about" id="about" aria-labelledby="aboutTitle">
                <h2 id="aboutTitle">A little curiosity.<br />A kitchen full of possibilities.</h2>
                <div><p>Cooking LAH brings food and culture to the same table. Explore the map, discover what people cook around the world, and find something new to try in your own kitchen.</p>
                <p>You don&rsquo;t need a fancy kitchen. Just an apron, an appetite, and somewhere to start.</p></div>
            </section>

            <script>
                document.addEventListener('DOMContentLoaded', function () {
                    var header = document.querySelector('.public-header');
                    if (!header) return;
                    function update() {
                        header.classList.toggle('is-scrolled', window.scrollY > 40);
                    }
                    window.addEventListener('scroll', update, { passive: true });
                    update();
                });
            </script>

            
</asp:Content>
<asp:Content ID="HomeFooter" ContentPlaceHolderID="FooterContent" runat="server">
    <footer class="site-footer">
        <a class="site-logo" runat="server" href="~/Main.aspx"><img runat="server" src="~/F_Assets/CookingLahLogo.png" alt="Cooking LAH home" /></a>
        <span>Good food. New places. Your kitchen.</span>
    </footer>


</asp:Content>
