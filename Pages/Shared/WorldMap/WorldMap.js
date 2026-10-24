/* ==========================================================
   COOKING LAH - WORLD MAP CONTROL  (WorldMap.js)
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
let selectedPath, mapMarker, markerLabel, markerBackground;
let mapZoom = 1;
const countryAnchors = new WeakMap();

// This map uses closed M/L/Z polygons. Ignore detached islands when finding
// the visual center so Alaska, overseas regions and the date line cannot skew it.
function getCountryAnchor(path) {
    if (countryAnchors.has(path)) return countryAnchors.get(path);
    const polygons = path.getAttribute("d").match(/M[^M]+/g).map(part => {
        const coordinates = part.match(/-?\d+(?:\.\d+)?/g).map(Number);
        const points = [];
        for (let i = 0; i < coordinates.length; i += 2) {
            points.push({ x: coordinates[i], y: coordinates[i + 1] });
        }
        let area = 0, x = 0, y = 0;
        points.forEach((a, i) => {
            const b = points[(i + 1) % points.length];
            const cross = a.x * b.y - b.x * a.y;
            area += cross;
            x += (a.x + b.x) * cross;
            y += (a.y + b.y) * cross;
        });
        return { points, area: Math.abs(area), center: { x: x / (3 * area), y: y / (3 * area) } };
    }).sort((a, b) => b.area - a.area);
    const polygon = polygons[0];
    // Signed distance to the coastline also handles concave countries whose
    // area centroid falls outside their borders.
    function clearance(point) {
        let inside = false, distance = Infinity;
        polygon.points.forEach((a, i) => {
            const b = polygon.points[(i + 1) % polygon.points.length];
            if ((a.y > point.y) !== (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            const dx = b.x - a.x, dy = b.y - a.y;
            const length = dx * dx + dy * dy;
            const t = length ? Math.max(0, Math.min(1, ((point.x - a.x) * dx + (point.y - a.y) * dy) / length)) : 0;
            distance = Math.min(distance, Math.hypot(point.x - a.x - t * dx, point.y - a.y - t * dy));
        });
        return inside ? distance : -distance;
    }
    let point = polygon.center;
    if (clearance(point) <= 0) {
        let left = Math.min(...polygon.points.map(p => p.x));
        let top = Math.min(...polygon.points.map(p => p.y));
        let width = Math.max(...polygon.points.map(p => p.x)) - left;
        let height = Math.max(...polygon.points.map(p => p.y)) - top;
        let best = -Infinity;
        for (let pass = 0; pass < 4; pass++) {
            for (let x = 0; x <= 24; x++) {
                for (let y = 0; y <= 24; y++) {
                    const candidate = { x: left + width * x / 24, y: top + height * y / 24 };
                    const distance = clearance(candidate);
                    if (distance > best) { best = distance; point = candidate; }
                }
            }
            width /= 6;
            height /= 6;
            left = point.x - width / 2;
            top = point.y - height / 2;
        }
    }
    countryAnchors.set(path, point);
    return point;
}

function updateMapMarker() {
    if (!selectedPath || !mapMarker) return;
    const point = getCountryAnchor(selectedPath);
    const scale = 1000 / worldMap.getBoundingClientRect().width;
    mapMarker.setAttribute("transform", `translate(${point.x} ${point.y}) scale(${scale})`);
    markerLabel.textContent = displayCountryName(selectedPath.dataset.n);
    const width = markerLabel.getComputedTextLength() + 24;
    const labelX = Math.max(-point.x / scale + 4, Math.min(-width / 2, (1000 - point.x) / scale - width - 4));
    const labelY = point.y / scale < 70 ? 10 : -64;
    markerBackground.setAttribute("x", labelX);
    markerBackground.setAttribute("y", labelY);
    markerBackground.setAttribute("width", width);
    markerLabel.setAttribute("x", labelX + width / 2);
    markerLabel.setAttribute("y", labelY + 20);
}

function initializeMapPanning(surface) {
    let drag = null;
    let suppressClick = false;
    surface.addEventListener("pointerdown", event => {
        suppressClick = false;
        // Touch uses the browser's native scrolling and pinch zoom.
        if (event.pointerType === "touch" || !event.isPrimary || event.button !== 0) return;
        if (surface.scrollWidth <= surface.clientWidth && surface.scrollHeight <= surface.clientHeight) return;
        drag = { id: event.pointerId, x: event.clientX, y: event.clientY,
            left: surface.scrollLeft, top: surface.scrollTop, moved: false };
    });
    surface.addEventListener("pointermove", event => {
        if (!drag || event.pointerId !== drag.id) return;
        const dx = event.clientX - drag.x;
        const dy = event.clientY - drag.y;
        if (!drag.moved && Math.hypot(dx, dy) < 5) return;
        if (!drag.moved) {
            drag.moved = true;
            surface.setPointerCapture(event.pointerId);
            surface.classList.add("is-dragging");
            hideMapTooltip();
        }
        event.preventDefault();
        surface.scrollLeft = drag.left - dx;
        surface.scrollTop = drag.top - dy;
    });
    function finishDrag(event) {
        if (!drag || event.pointerId !== drag.id) return;
        suppressClick = drag.moved;
        drag = null;
        surface.classList.remove("is-dragging");
        if (surface.hasPointerCapture(event.pointerId)) surface.releasePointerCapture(event.pointerId);
    }
    surface.addEventListener("pointerup", finishDrag);
    surface.addEventListener("pointercancel", finishDrag);
    surface.addEventListener("lostpointercapture", finishDrag);
    surface.addEventListener("pointerleave", event => {
        if (drag && !drag.moved) finishDrag(event);
    });
    surface.addEventListener("click", event => {
        if (!suppressClick || event.detail === 0) return;
        event.preventDefault();
        event.stopPropagation();
        suppressClick = false;
    }, true);
    surface.addEventListener("dragstart", event => event.preventDefault());
}

function initializeMapControls() {
    const ns = "http://www.w3.org/2000/svg";
    mapMarker = document.createElementNS(ns, "g");
    mapMarker.classList.add("map-selection-marker");
    mapMarker.setAttribute("aria-hidden", "true");
    mapMarker.innerHTML = '<path d="M0 0C-4-6-12-14-12-23a12 12 0 0 1 24 0C12-14 4-6 0 0Z"/><circle cx="0" cy="-23" r="4"/><rect height="30" rx="10"/><text text-anchor="middle"></text>';
    worldMap.appendChild(mapMarker);
    markerBackground = mapMarker.querySelector("rect");
    markerLabel = mapMarker.querySelector("text");
    const surface = worldMap.parentElement;
    initializeMapPanning(surface);
    const dialog = document.getElementById("mapDialog");
    const enlarge = document.getElementById("enlargeMap");
    const home = document.createComment("Map position");
    mapStage.before(home);
    enlarge.addEventListener("click", () => {
        hideMapTooltip();
        if (dialog.open) {
            dialog.close();
        } else {
            dialog.appendChild(mapStage);
            dialog.showModal();
            document.body.classList.add("map-dialog-open");
            enlarge.textContent = "Close enlarged map";
            enlarge.focus();
        }
    });
    dialog.addEventListener("close", () => {
        home.after(mapStage);
        document.body.classList.remove("map-dialog-open");
        enlarge.textContent = "Enlarge map";
        enlarge.focus();
    });
    function setZoom(value) {
        mapZoom = Math.max(1, Math.min(3, value));
        worldMap.style.width = (mapZoom * 100) + "%";
        document.getElementById("mapZoomLevel").textContent = Math.round(mapZoom * 100) + "%";
        document.getElementById("mapZoomOut").disabled = mapZoom === 1;
        document.getElementById("mapZoomIn").disabled = mapZoom === 3;
        hideMapTooltip();
        updateMapMarker();
        if (selectedPath) selectedPath.scrollIntoView({ block: "nearest", inline: "center" });
    }
    document.getElementById("mapZoomIn").addEventListener("click", () => setZoom(mapZoom + 0.5));
    document.getElementById("mapZoomOut").addEventListener("click", () => setZoom(mapZoom - 0.5));
    document.getElementById("mapZoomReset").addEventListener("click", () => setZoom(1));
    document.getElementById("mapZoomOut").disabled = true;
    surface.addEventListener("scroll", hideMapTooltip);
    new ResizeObserver(updateMapMarker).observe(worldMap);
}

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
    document.querySelectorAll("#worldMap path.is-selected").forEach(el => el.classList.remove("is-selected"));
    countryChips.querySelectorAll(".is-active").forEach(el => {
        el.classList.remove("is-active");
        el.setAttribute("aria-pressed", "false");
    });

    const path = worldMap.querySelector('path[data-n="' + countryName + '"]');
    if (path) path.classList.add("is-selected");
    selectedPath = path;
    updateMapMarker();
    hideMapTooltip();

    const chip = countryChips.querySelector('[data-n="' + countryName + '"]');
    if (chip) {
        chip.classList.add("is-active");
        chip.setAttribute("aria-pressed", "true");
    }

    if (foodData[countryName]) updateCountryCard(countryName);
    else showComingSoon(countryName);
}

function showMapTooltip(event, countryName) {
    if (worldMap.parentElement.classList.contains("is-dragging")) return;
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
    worldMap.querySelectorAll("path[data-n]").forEach(path => {
        const name = path.dataset.n;
        const hasFood = Boolean(foodData[name]);

        path.setAttribute("class", hasFood ? "has" : "no");

        path.setAttribute("tabindex", "0");
        path.setAttribute("role", "button");
        path.setAttribute("aria-label", name + ": " + (hasFood ? foodData[name][0] : "Coming soon"));
        path.addEventListener("click", () => selectCountry(name));
        path.addEventListener("keydown", event => {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                selectCountry(name);
            }
        });

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
    initializeMapControls();
    initializeCountryChips();
    selectCountry("Malaysia");
});
