(function () {
    'use strict';

    var worldMap;
    var viewport;
    var tooltip;
    var zoomLevel;
    var zoomInButton;
    var zoomOutButton;
    var zoomResetButton;

    var selectedPath = null;
    var mapZoom = 1;
    var minZoom = 1;
    var maxZoom = 6;
    var centerX = 500, centerY = 252;
    var baseWidth = 1040, baseHeight = 544;
    var zoomStep = 0.2;

    var countryAnchors = new WeakMap();

    function clamp(value, min, max) {
        return Math.max(min, Math.min(max, value));
    }

    function displayCountryName(name) {
        if (name === 'United States of America') {
            return 'United States';
        }

        return name;
    }

    function getCountryAnchor(path) {
        if (countryAnchors.has(path)) {
            return countryAnchors.get(path);
        }

        var parts = path.getAttribute('d').match(/M[^M]+/g) || [];
        var polygons = parts.map(function (part) {
            var coordinates = (part.match(/-?\d+(?:\.\d+)?/g) || []).map(Number);
            var points = [];

            for (var i = 0; i < coordinates.length; i += 2) {
                points.push({
                    x: coordinates[i],
                    y: coordinates[i + 1]
                });
            }

            var area = 0;
            var x = 0;
            var y = 0;

            points.forEach(function (a, index) {
                var b = points[(index + 1) % points.length];
                var cross = a.x * b.y - b.x * a.y;

                area += cross;
                x += (a.x + b.x) * cross;
                y += (a.y + b.y) * cross;
            });

            return {
                points: points,
                area: Math.abs(area),
                center: {
                    x: area ? x / (3 * area) : 500,
                    y: area ? y / (3 * area) : 252
                }
            };
        }).sort(function (a, b) {
            return b.area - a.area;
        });

        var polygon = polygons[0];
        var point = polygon ? polygon.center : { x: 500, y: 252 };

        countryAnchors.set(path, point);
        return point;
    }

    function hideTooltip() {
        if (tooltip) {
            tooltip.style.display = 'none';
        }
    }

    function showTooltip(event, countryName) {
        if (!tooltip || viewport.classList.contains('is-dragging')) return;
        var rect = viewport.getBoundingClientRect();
        tooltip.textContent = displayCountryName(countryName);
        tooltip.style.display = 'block';
        tooltip.style.left = clamp(event.clientX - rect.left + 12, 8, viewport.clientWidth - tooltip.offsetWidth - 8) + 'px';
        tooltip.style.top = clamp(event.clientY - rect.top + 16, 8, viewport.clientHeight - tooltip.offsetHeight - 8) + 'px';
    }

    function updateZoomControls() {
        zoomLevel.textContent = Math.round(mapZoom * 100) + '%';
        zoomOutButton.disabled = mapZoom <= minZoom;
        zoomInButton.disabled = mapZoom >= maxZoom;
    }

    function renderCamera() {
        var width = baseWidth / mapZoom, height = baseHeight / mapZoom;
        centerX = width >= 1040 ? 500 : clamp(centerX, -20 + width / 2, 1020 - width / 2);
        centerY = height >= 544 ? 252 : clamp(centerY, -20 + height / 2, 524 - height / 2);
        worldMap.setAttribute('viewBox', [centerX - width / 2, centerY - height / 2, width, height].join(' '));
        updateZoomControls();
        hideTooltip();
    }

    function resizeMap() {
        if (!viewport.clientWidth || !viewport.clientHeight) return;
        var ratio = viewport.clientWidth / viewport.clientHeight;
        baseWidth = Math.max(1040, 544 * ratio);
        baseHeight = baseWidth / ratio;
        renderCamera();
    }

    // Zoom the SVG camera, keeping the point under the cursor in place.
    function setZoom(value, pointerX, pointerY) {
        var nextZoom = clamp(value, minZoom, maxZoom);
        var x = typeof pointerX === 'number' ? pointerX / viewport.clientWidth - 0.5 : 0;
        var y = typeof pointerY === 'number' ? pointerY / viewport.clientHeight - 0.5 : 0;
        centerX += x * (baseWidth / mapZoom - baseWidth / nextZoom);
        centerY += y * (baseHeight / mapZoom - baseHeight / nextZoom);
        mapZoom = nextZoom;
        renderCamera();
    }

    function focusCountry(countryName) {
        var path = worldMap.querySelector('path[data-n="' + CSS.escape(countryName) + '"]');
        if (!path) return false;
        var point = getCountryAnchor(path);
        centerX = point.x;
        centerY = point.y;
        mapZoom = Math.max(mapZoom, 1.8);
        renderCamera();
        return true;
    }

    function dispatchCountryChange(countryName) {
        document.dispatchEvent(
            new CustomEvent('worldmap:countrychange', {
                detail: {
                    country: countryName
                }
            })
        );
    }

    function selectCountry(countryName, options) {
        options = options || {};

        var path = worldMap.querySelector('path[data-n="' + CSS.escape(countryName) + '"]');

        if (!path) {
            return false;
        }

        if (selectedPath) {
            selectedPath.classList.remove('is-selected');
            selectedPath.setAttribute('aria-pressed', 'false');
        }

        selectedPath = path;
        selectedPath.classList.add('is-selected');
        selectedPath.setAttribute('aria-pressed', 'true');
        document.getElementById('mapSelection').textContent = displayCountryName(countryName);

        if (options.focus) {
            focusCountry(countryName, options.smooth);
        }

        if (options.emit !== false) {
            dispatchCountryChange(countryName);
        }

        return true;
    }

    function initializeCountries() {
        worldMap.querySelectorAll('path[data-n]').forEach(function (path) {
            var countryName = path.dataset.n;

            path.classList.add('map-country');
            path.setAttribute('tabindex', '0');
            path.setAttribute('role', 'button');
            path.setAttribute('aria-pressed', 'false');
            path.setAttribute('aria-label', displayCountryName(countryName));

            path.addEventListener('click', function () {
                selectCountry(countryName, {
                    emit: true,
                    focus: false
                });
            });

            path.addEventListener('keydown', function (event) {
                if (event.key !== 'Enter' && event.key !== ' ') {
                    return;
                }

                event.preventDefault();

                selectCountry(countryName, {
                    emit: true,
                    focus: false
                });
            });

            path.addEventListener('mousemove', function (event) {
                showTooltip(event, countryName);
            });

            path.addEventListener('mouseleave', hideTooltip);
        });
    }

    function initializePanning() {
        var drag = null;
        var suppressClick = false;

        viewport.addEventListener('pointerdown', function (event) {
            suppressClick = false;

            if (!event.isPrimary || event.button !== 0) {
                return;
            }

            drag = {
                id: event.pointerId,
                x: event.clientX,
                y: event.clientY,
                left: centerX,
                top: centerY,
                moved: false
            };
        });

        viewport.addEventListener('pointermove', function (event) {
            if (!drag || event.pointerId !== drag.id) {
                return;
            }

            var dx = event.clientX - drag.x;
            var dy = event.clientY - drag.y;

            if (!drag.moved && Math.hypot(dx, dy) < 5) {
                return;
            }

            if (!drag.moved) {
                drag.moved = true;
                viewport.setPointerCapture(event.pointerId);
                viewport.classList.add('is-dragging');
                hideTooltip();
            }

            event.preventDefault();
            centerX = drag.left - dx * baseWidth / mapZoom / viewport.clientWidth;
            centerY = drag.top - dy * baseHeight / mapZoom / viewport.clientHeight;
            renderCamera();
        });

        function finishDrag(event) {
            if (!drag || event.pointerId !== drag.id) {
                return;
            }

            suppressClick = drag.moved;
            drag = null;
            viewport.classList.remove('is-dragging');

            if (viewport.hasPointerCapture(event.pointerId)) {
                viewport.releasePointerCapture(event.pointerId);
            }
        }

        viewport.addEventListener('pointerup', finishDrag);
        viewport.addEventListener('pointercancel', finishDrag);
        viewport.addEventListener('lostpointercapture', finishDrag);

        viewport.addEventListener('click', function (event) {
            if (!suppressClick || event.detail === 0) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();
            suppressClick = false;
        }, true);

        viewport.addEventListener('dragstart', function (event) {
            event.preventDefault();
        });
    }

    function initializeWheelZoom() {
        viewport.addEventListener('wheel', function (event) {
            if (event.deltaY === 0) return;
            event.preventDefault();

            var rect = viewport.getBoundingClientRect();
            var pointerX = event.clientX - rect.left;
            var pointerY = event.clientY - rect.top;
            var direction = event.deltaY < 0 ? 1 : -1;

            setZoom(mapZoom + direction * zoomStep, pointerX, pointerY);
        }, { passive: false });
    }

    function getCountries() {
        return Array.from(worldMap.querySelectorAll('path[data-n]'))
            .map(function (path) {
                return path.dataset.n;
            })
            .filter(function (name, index, values) {
                return values.indexOf(name) === index;
            })
            .sort(function (a, b) {
                return displayCountryName(a).localeCompare(displayCountryName(b));
            });
    }

    function initialize() {
        worldMap = document.getElementById('worldMap');
        viewport = document.getElementById('mapViewport');
        tooltip = document.getElementById('mapTooltip');
        zoomLevel = document.getElementById('mapZoomLevel');
        zoomInButton = document.getElementById('mapZoomIn');
        zoomOutButton = document.getElementById('mapZoomOut');
        zoomResetButton = document.getElementById('mapZoomReset');

        if (!worldMap || !viewport) {
            return;
        }

        initializeCountries();
        initializePanning();
        initializeWheelZoom();
        updateZoomControls();

        zoomInButton.addEventListener('click', function () {
            setZoom(mapZoom + 0.5);
        });

        zoomOutButton.addEventListener('click', function () {
            setZoom(mapZoom - 0.5);
        });

        zoomResetButton.addEventListener('click', function () {
            centerX = 500;
            centerY = 252;
            setZoom(1);
        });
        viewport.addEventListener('keydown', function (event) {
            if (event.target !== viewport) return;
            var amount = 70 / mapZoom;
            if (event.key === 'ArrowLeft') centerX -= amount;
            else if (event.key === 'ArrowRight') centerX += amount;
            else if (event.key === 'ArrowUp') centerY -= amount;
            else if (event.key === 'ArrowDown') centerY += amount;
            else if (event.key === '+' || event.key === '=') { event.preventDefault(); setZoom(mapZoom + zoomStep); return; }
            else if (event.key === '-') { event.preventDefault(); setZoom(mapZoom - zoomStep); return; }
            else return;
            event.preventDefault();
            renderCamera();
        });
        resizeMap();
        new ResizeObserver(resizeMap).observe(viewport);

        window.CookingLahWorldMap = {
            getCountries: getCountries,
            selectCountry: selectCountry,
            focusCountry: focusCountry,
            getSelectedCountry: function () {
                return selectedPath ? selectedPath.dataset.n : '';
            }
        };

        document.dispatchEvent(new CustomEvent('worldmap:ready'));
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }
}());
