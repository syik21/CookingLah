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

                </div>
                    <div class="main-falling" id="mainFalling" aria-hidden="true">
                        <img runat="server" src="~/F_Assets/egg.png"   alt="" style="--x:6%;  --s:180px; --ar:1.05; --b:15px; --rot:-8deg;  --d:1.8s; --dl:0s" />
                        <img runat="server" src="~/F_Assets/milk.png"   alt="" style="--x:18%; --s:210px; --ar:1.12; --b:4px;  --rot:10deg;  --d:2.2s; --dl:.35s" />
                        <img runat="server" src="~/F_Assets/lettuce.png" alt="" style="--x:28%; --s:220px; --ar:0.835; --b:14px; --rot:-6deg;  --d:2s;   --dl:.15s" />
                        <img runat="server" src="~/F_Assets/toast.png"    alt="" style="--x:42%; --s:200px; --ar:0.853; --b:6px;  --rot:7deg;   --d:2.4s; --dl:.6s" />
                        <img runat="server" src="~/F_Assets/tomato.png"   alt="" style="--x:54%; --s:160px; --ar:0.77; --b:20px; --rot:12deg;  --d:1.9s; --dl:.9s" />
                        <img runat="server" src="~/F_Assets/corn.png"   alt="" style="--x:65%; --s:140px; --ar:0.87; --b:6px;  --rot:-12deg; --d:2.3s; --dl:.25s" />
                        <img runat="server" src="~/F_Assets/potato.png"    alt="" style="--x:73%; --s:200px; --ar:0.83; --b:12px; --rot:-11deg;  --d:2.1s; --dl:.75s" />
                        <img runat="server" src="~/F_Assets/onion.png" alt="" style="--x:85%; --s:140px; --ar:0.855; --b:8px;  --rot:9deg;   --d:2.2s; --dl:.45s" />
                    </div>

                <script>
                    (function () {
                        var box = document.getElementById('mainFalling'), away = false;
                        if (!box) return;
                        function drop() {
                            box.classList.remove('is-dropping');
                            void box.offsetWidth;                 // restart the animation
                            box.classList.add('is-dropping');
                        }
                        window.addEventListener('load', drop);
                        window.addEventListener('scroll', function () {
                            if (window.scrollY > 300) away = true;            // scrolled down the page
                            else if (away && window.scrollY < 120) {          // came back to the top
                                away = false; drop();
                            }
                        }, { passive: true });
                    })();
                </script>
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

            
<section class="container boxes" aria-label="Follow and links">
    <div class="box scrap-paper social-box">
        <img runat="server" class="social-avatar" src="~/F_Assets/xx.png" alt="@_nixgnaix profile picture" />
        <a class="social-link" href="https://www.instagram.com/_nixgnaix/" target="_blank" rel="noopener noreferrer">
            <img runat="server" class="social-ig-icon" src="~/F_Assets/instagram-icon.png" alt="" />
            <span class="social-handle">@_nixgnaix</span>
        </a>
    </div>

    <div class="box scrap-paper social-box">
        <img runat="server" class="social-avatar" src="~/F_Assets/sy.png" alt="@_seryik profile picture" />
        <a class="social-link" href="https://www.instagram.com/_seryik/" target="_blank" rel="noopener noreferrer">
            <img runat="server" class="social-ig-icon" src="~/F_Assets/instagram-icon.png" alt="" />
            <span class="social-handle">@_seryik</span>
        </a>
    </div>

    <div class="box scrap-paper social-box">
        <img runat="server" class="social-avatar" src="~/F_Assets/wh.png" alt="@wenhui_1123 profile picture" />
        <a class="social-link" href="https://www.instagram.com/wenhui_1123/" target="_blank" rel="noopener noreferrer">
            <img runat="server" class="social-ig-icon" src="~/F_Assets/instagram-icon.png" alt="" />
            <span class="social-handle">@wenhui_1123</span>
        </a>
    </div>
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