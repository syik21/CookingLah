/* ==========================================================
   COOKING LAH - MAIN PAGE  (Main.js)
   Interactive world food map. Vanilla JS, no dependencies.
   ========================================================== */

/* Food data: "Country name (matches data-n in the SVG)": [dish, description, [ingredients]] */
const foodData = {
    "Malaysia": ["Nasi lemak", "Coconut rice with sambal, egg, peanuts and crispy anchovies. Eaten at breakfast, lunch and midnight.", ["coconut", "sambal", "pandan"]],
    "Thailand": ["Pad kra pao", "Stir-fried meat with holy basil and chilli, served over rice with a crispy fried egg.", ["holy basil", "chilli", "fish sauce"]],
    "Japan": ["Okonomiyaki", "A savoury cabbage pancake cooked on a griddle, topped with sauce, mayo and bonito flakes.", ["cabbage", "bonito", "dashi"]],
    "China": ["Mapo tofu", "Silky tofu in a numbing, spicy sauce. Sichuan pepper is the point.", ["Sichuan pepper", "doubanjiang", "tofu"]],
    "South Korea": ["Kimchi jjigae", "A bubbling stew of aged kimchi and pork. Sour, spicy and made for cold days.", ["kimchi", "gochugaru", "pork"]],
    "Vietnam": ["Pho", "A slow-simmered beef broth with rice noodles and a plate of fresh herbs on the side.", ["star anise", "rice noodles", "herbs"]],
    "Indonesia": ["Nasi goreng", "Smoky fried rice with sweet soy sauce, shallots and a runny egg.", ["kecap manis", "shallot", "egg"]],
    "Philippines": ["Chicken adobo", "Chicken braised in vinegar, soy sauce and garlic until sticky and tangy.", ["vinegar", "garlic", "bay leaf"]],
    "India": ["Dal tadka", "Yellow lentils finished with sizzling spiced oil. The comfort food of a billion people.", ["lentils", "cumin", "ghee"]],
    "Pakistan": ["Chicken karahi", "Chicken cooked fast in a wok with tomatoes, ginger and green chilli.", ["tomato", "ginger", "green chilli"]],
    "Iran": ["Ghormeh sabzi", "A deep green herb stew with kidney beans and dried limes, served with saffron rice.", ["herbs", "dried lime", "saffron"]],
    "Turkey": ["Menemen", "Eggs scrambled with tomatoes and peppers, scooped up with plenty of bread.", ["egg", "pepper", "bread"]],
    "Lebanon": ["Mezze spread", "Many small plates: hummus, tabbouleh and grilled halloumi, shared at one table.", ["chickpeas", "parsley", "olive oil"]],
    "Egypt": ["Koshari", "Rice, lentils and pasta piled high, finished with tomato sauce and crispy onions.", ["lentils", "tomato", "fried onion"]],
    "Morocco": ["Chicken tagine", "Slow-cooked chicken with preserved lemon and olives in a warm spice blend.", ["preserved lemon", "ras el hanout", "olives"]],
    "Ethiopia": ["Doro wat", "A rich chicken stew with berbere, eaten with spongy injera by hand.", ["berbere", "injera", "egg"]],
    "Nigeria": ["Jollof rice", "Rice cooked in a spiced tomato base until smoky. A friendly national argument.", ["tomato", "scotch bonnet", "rice"]],
    "South Africa": ["Bobotie", "Spiced minced meat baked under a savoury egg custard.", ["curry", "apricot", "egg"]],
    "Italy": ["Cacio e pepe", "Pasta, pecorino and black pepper. Three ingredients and no room to hide.", ["pecorino", "pepper", "pasta water"]],
    "France": ["Ratatouille", "Summer vegetables stewed slowly in olive oil until sweet and soft.", ["aubergine", "courgette", "thyme"]],
    "Spain": ["Tortilla española", "A thick potato and onion omelette served warm or cold, any time of day.", ["potato", "onion", "egg"]],
    "Greece": ["Spanakopita", "Flaky filo pie filled with spinach, feta and dill.", ["filo", "feta", "dill"]],
    "Germany": ["Käsespätzle", "Soft egg noodles layered with melted cheese and crispy onions.", ["spätzle", "cheese", "onion"]],
    "United Kingdom": ["Sunday roast", "Roast meat, crispy potatoes and gravy, shared on the last day of the weekend.", ["potato", "gravy", "Yorkshire pudding"]],
    "Russia": ["Borscht", "A bright beetroot soup finished with sour cream and dill.", ["beetroot", "cabbage", "sour cream"]],
    "United States of America": ["Smoked brisket", "Beef cooked low and slow over wood until it falls apart. Texas is serious about it.", ["beef", "smoke", "pepper"]],
    "Canada": ["Poutine", "Fries topped with cheese curds and hot gravy. Messy and worth it.", ["curds", "gravy", "fries"]],
    "Mexico": ["Tacos al pastor", "Marinated pork with pineapple on small tortillas, finished with onion and coriander.", ["achiote", "pineapple", "tortilla"]],
    "Peru": ["Ceviche", "Raw fish cured in lime with chilli and red onion. Ready in minutes.", ["lime", "aji", "red onion"]],
    "Brazil": ["Feijoada", "A slow pot of black beans and pork, served with rice, greens and orange.", ["black beans", "pork", "collards"]],
    "Argentina": ["Asado", "Beef grilled over embers and shared with chimichurri on the side.", ["beef", "chimichurri", "embers"]],
    "Australia": ["Lamingtons", "Sponge cake squares dipped in chocolate and rolled in coconut.", ["sponge", "chocolate", "coconut"]]
};

const COMING_SOON_TEXT = "We have not cooked this country yet. Pick a tan one, or tell us what to try next.";
const CHIP_LABELS = { "United States of America": "USA", "United Kingdom": "UK" };

let mapStage, worldMap, mapTooltip, countryChips;
let countryNameEl, dishNameEl, countryDescriptionEl, countryTagsEl;

function displayCountryName(name) {
    return name.replace("United States of America", "United States");
}

function updateCountryCard(countryName) {
    const entry = foodData[countryName];
    if (!entry) return;
    countryNameEl.textContent = displayCountryName(countryName);
    dishNameEl.textContent = entry[0];
    countryDescriptionEl.textContent = entry[1];
    countryTagsEl.replaceChildren(...entry[2].map(text => {
        const tag = document.createElement("span");
        tag.className = "tag";
        tag.textContent = text;
        return tag;
    }));
}

function showComingSoon(countryName) {
    countryNameEl.textContent = countryName;
    dishNameEl.textContent = "Coming soon";
    countryDescriptionEl.textContent = COMING_SOON_TEXT;
    countryTagsEl.replaceChildren();
}

function selectCountry(countryName) {
    if (!foodData[countryName]) return;

    document.querySelectorAll("#worldMap path.is-selected").forEach(el => el.classList.remove("is-selected"));
    countryChips.querySelectorAll(".is-active").forEach(el => {
        el.classList.remove("is-active");
        el.setAttribute("aria-pressed", "false");
    });

    const path = worldMap.querySelector('path[data-n="' + countryName + '"]');
    if (path) path.classList.add("is-selected");

    const chip = countryChips.querySelector('[data-n="' + countryName + '"]');
    if (chip) {
        chip.classList.add("is-active");
        chip.setAttribute("aria-pressed", "true");
    }

    updateCountryCard(countryName);
}

function showMapTooltip(event, countryName) {
    const rect = mapStage.getBoundingClientRect();
    mapTooltip.style.display = "block";
    mapTooltip.style.left = (event.clientX - rect.left) + "px";
    mapTooltip.style.top = (event.clientY - rect.top) + "px";
    mapTooltip.textContent = displayCountryName(countryName);
}

function hideMapTooltip() {
    mapTooltip.style.display = "none";
}

function initializeMap() {
    worldMap.querySelectorAll("path").forEach(path => {
        const name = path.dataset.n;
        const hasFood = Boolean(foodData[name]);

        path.setAttribute("class", hasFood ? "has" : "no");

        if (hasFood) {
            path.setAttribute("tabindex", "0");
            path.setAttribute("role", "button");
            path.setAttribute("aria-label", name + ": " + foodData[name][0]);
            path.addEventListener("click", () => selectCountry(name));
            path.addEventListener("keydown", event => {
                if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    selectCountry(name);
                }
            });
        } else {
            path.addEventListener("click", () => showComingSoon(name));
        }

        path.addEventListener("mousemove", event => showMapTooltip(event, name));
        path.addEventListener("mouseleave", hideMapTooltip);
    });
}

function initializeCountryChips() {
    Object.keys(foodData).sort().forEach(name => {
        const chip = document.createElement("button");
        chip.type = "button";               /* never submit the Web Forms <form> */
        chip.className = "pill";
        chip.dataset.n = name;
        chip.setAttribute("aria-pressed", "false");
        chip.textContent = CHIP_LABELS[name] || name;
        chip.addEventListener("click", () => selectCountry(name));
        countryChips.appendChild(chip);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    mapStage = document.getElementById("mapStage");
    worldMap = document.getElementById("worldMap");
    mapTooltip = document.getElementById("mapTooltip");
    countryChips = document.getElementById("countryChips");
    countryNameEl = document.getElementById("countryName");
    dishNameEl = document.getElementById("dishName");
    countryDescriptionEl = document.getElementById("countryDescription");
    countryTagsEl = document.getElementById("countryTags");

    initializeMap();
    initializeCountryChips();
    selectCountry("Malaysia");
});