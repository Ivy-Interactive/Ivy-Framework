/**
 * Leaflet CSS styles that will be injected at runtime.
 * This allows the external widget to work without a separate CSS file.
 */

const LEAFLET_CSS = `
/* Leaflet core styles */
.leaflet-pane,
.leaflet-tile,
.leaflet-marker-icon,
.leaflet-marker-shadow,
.leaflet-tile-container,
.leaflet-pane > svg,
.leaflet-pane > canvas,
.leaflet-zoom-box,
.leaflet-image-layer,
.leaflet-layer {
  position: absolute;
  left: 0;
  top: 0;
}

.leaflet-container {
  overflow: hidden;
}

.leaflet-tile,
.leaflet-marker-icon,
.leaflet-marker-shadow {
  -webkit-user-select: none;
  -moz-user-select: none;
  user-select: none;
  -webkit-user-drag: none;
}

.leaflet-tile::selection {
  background: transparent;
}

.leaflet-safari .leaflet-tile {
  image-rendering: -webkit-optimize-contrast;
}

.leaflet-safari .leaflet-tile-container {
  width: 1600px;
  height: 1600px;
  -webkit-transform-origin: 0 0;
}

.leaflet-marker-icon,
.leaflet-marker-shadow {
  display: block;
}

.leaflet-container .leaflet-overlay-pane svg {
  max-width: none !important;
  max-height: none !important;
}

.leaflet-container .leaflet-marker-pane img,
.leaflet-container .leaflet-shadow-pane img,
.leaflet-container .leaflet-tile-pane img,
.leaflet-container img.leaflet-image-layer,
.leaflet-container .leaflet-tile {
  max-width: none !important;
  max-height: none !important;
  width: auto;
  padding: 0;
}

.leaflet-container.leaflet-touch-zoom {
  -ms-touch-action: pan-x pan-y;
  touch-action: pan-x pan-y;
}

.leaflet-container.leaflet-touch-drag {
  -ms-touch-action: pinch-zoom;
  touch-action: none;
  touch-action: pinch-zoom;
}

.leaflet-container.leaflet-touch-drag.leaflet-touch-zoom {
  -ms-touch-action: none;
  touch-action: none;
}

.leaflet-container {
  -webkit-tap-highlight-color: transparent;
}

.leaflet-container a {
  -webkit-tap-highlight-color: rgba(51, 181, 229, 0.4);
}

.leaflet-tile {
  filter: inherit;
  visibility: hidden;
}

.leaflet-tile-loaded {
  visibility: inherit;
}

.leaflet-zoom-box {
  width: 0;
  height: 0;
  -moz-box-sizing: border-box;
  box-sizing: border-box;
  z-index: 800;
}

.leaflet-overlay-pane svg {
  -moz-user-select: none;
}

.leaflet-pane {
  z-index: 400;
}

.leaflet-tile-pane {
  z-index: 200;
}

.leaflet-overlay-pane {
  z-index: 400;
}

.leaflet-shadow-pane {
  z-index: 500;
}

.leaflet-marker-pane {
  z-index: 600;
}

.leaflet-tooltip-pane {
  z-index: 650;
}

.leaflet-popup-pane {
  z-index: 700;
}

.leaflet-map-pane canvas {
  z-index: 100;
}

.leaflet-map-pane svg {
  z-index: 200;
}

.leaflet-vml-shape {
  width: 1px;
  height: 1px;
}

.lvml {
  behavior: url(#default#VML);
  display: inline-block;
  position: absolute;
}

.leaflet-control {
  position: relative;
  z-index: 800;
  pointer-events: visiblePainted;
  pointer-events: auto;
}

.leaflet-top,
.leaflet-bottom {
  position: absolute;
  z-index: 1000;
  pointer-events: none;
}

.leaflet-top {
  top: 0;
}

.leaflet-right {
  right: 0;
}

.leaflet-bottom {
  bottom: 0;
}

.leaflet-left {
  left: 0;
}

.leaflet-control {
  float: left;
  clear: both;
}

.leaflet-right .leaflet-control {
  float: right;
}

.leaflet-top .leaflet-control {
  margin-top: 10px;
}

.leaflet-bottom .leaflet-control {
  margin-bottom: 10px;
}

.leaflet-left .leaflet-control {
  margin-left: 10px;
}

.leaflet-right .leaflet-control {
  margin-right: 10px;
}

.leaflet-fade-anim .leaflet-popup {
  opacity: 0;
  -webkit-transition: opacity 0.2s linear;
  -moz-transition: opacity 0.2s linear;
  transition: opacity 0.2s linear;
}

.leaflet-fade-anim .leaflet-map-pane .leaflet-popup {
  opacity: 1;
}

.leaflet-zoom-animated {
  -webkit-transform-origin: 0 0;
  -ms-transform-origin: 0 0;
  transform-origin: 0 0;
}

svg.leaflet-zoom-animated {
  will-change: transform;
}

.leaflet-zoom-anim .leaflet-zoom-animated {
  -webkit-transition: -webkit-transform 0.25s cubic-bezier(0, 0, 0.25, 1);
  -moz-transition: -moz-transform 0.25s cubic-bezier(0, 0, 0.25, 1);
  transition: transform 0.25s cubic-bezier(0, 0, 0.25, 1);
}

.leaflet-zoom-anim .leaflet-tile,
.leaflet-pan-anim .leaflet-tile {
  -webkit-transition: none;
  -moz-transition: none;
  transition: none;
}

.leaflet-zoom-anim .leaflet-zoom-hide {
  visibility: hidden;
}

.leaflet-interactive {
  cursor: pointer;
}

.leaflet-grab {
  cursor: -webkit-grab;
  cursor: -moz-grab;
  cursor: grab;
}

.leaflet-crosshair,
.leaflet-crosshair .leaflet-interactive {
  cursor: crosshair;
}

.leaflet-popup-pane,
.leaflet-control {
  cursor: auto;
}

.leaflet-dragging .leaflet-grab,
.leaflet-dragging .leaflet-grab .leaflet-interactive,
.leaflet-dragging .leaflet-marker-draggable {
  cursor: move;
  cursor: -webkit-grabbing;
  cursor: -moz-grabbing;
  cursor: grabbing;
}

.leaflet-marker-icon,
.leaflet-marker-shadow,
.leaflet-image-layer,
.leaflet-pane > svg path,
.leaflet-tile-container {
  pointer-events: none;
}

.leaflet-marker-icon.leaflet-interactive,
.leaflet-image-layer.leaflet-interactive,
.leaflet-pane > svg path.leaflet-interactive,
svg.leaflet-image-layer.leaflet-interactive path {
  pointer-events: visiblePainted;
  pointer-events: auto;
}

.leaflet-container {
  background: #ddd;
  outline-offset: 1px;
}

.leaflet-container a {
  color: #0078A8;
}

.leaflet-zoom-box {
  border: 2px dotted #38f;
  background: rgba(255, 255, 255, 0.5);
}

.leaflet-container {
  font-family: "Helvetica Neue", Arial, Helvetica, sans-serif;
  font-size: 12px;
  font-size: 0.75rem;
  line-height: 1.5;
}

.leaflet-bar {
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.65);
  border-radius: 4px;
}

.leaflet-bar a {
  background-color: #fff;
  border-bottom: 1px solid #ccc;
  width: 26px;
  height: 26px;
  line-height: 26px;
  display: block;
  text-align: center;
  text-decoration: none;
  color: black;
}

.leaflet-bar a,
.leaflet-control-layers-toggle {
  background-position: 50% 50%;
  background-repeat: no-repeat;
  display: block;
}

.leaflet-bar a:hover,
.leaflet-bar a:focus {
  background-color: #f4f4f4;
}

.leaflet-bar a:first-child {
  border-top-left-radius: 4px;
  border-top-right-radius: 4px;
}

.leaflet-bar a:last-child {
  border-bottom-left-radius: 4px;
  border-bottom-right-radius: 4px;
  border-bottom: none;
}

.leaflet-bar a.leaflet-disabled {
  cursor: default;
  background-color: #f4f4f4;
  color: #bbb;
}

.leaflet-touch .leaflet-bar a {
  width: 30px;
  height: 30px;
  line-height: 30px;
}

.leaflet-touch .leaflet-bar a:first-child {
  border-top-left-radius: 2px;
  border-top-right-radius: 2px;
}

.leaflet-touch .leaflet-bar a:last-child {
  border-bottom-left-radius: 2px;
  border-bottom-right-radius: 2px;
}

.leaflet-control-zoom-in,
.leaflet-control-zoom-out {
  font: bold 18px 'Lucida Console', Monaco, monospace;
  text-indent: 1px;
}

.leaflet-touch .leaflet-control-zoom-in,
.leaflet-touch .leaflet-control-zoom-out {
  font-size: 22px;
}

.leaflet-control-layers {
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.4);
  background: #fff;
  border-radius: 5px;
}

.leaflet-control-layers-toggle {
  background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3QYfFjIBzYr7NQAAAB1pVFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAABFklEQVRIx+2WMW6EMBCGP5x0NyhF2tx/S0FxlAIJRbyCJTqKNDlE7uAGKCi44BnSJDoPsGsLaRG3cUE2MBgDAgQ/8ozzz3g8Y5bYJMaY4+rrIzWUQl8wM6K8RzEJLZANWTl9kCQFMqoFVBLSXtg8cJLUUDqP7uLlHwp/B/4ySHOJdF7Y+AXYLmzOBpDuQ2RN1gkbn3P+CbDV2FxWkvL34GckzbUFNvvA1i9kDqQuEHMn5XxPEts3O4D0TyFy6iS+TrKk0F7EsS3yJEvqFDYblGwOG0n2JIWy+ktiCWx+wMbrJJu9oLgPbGzbT+IcDNsUkr9qk9g4xnNx7XfKRLwJxLNiMTawewGwX4DNnb3vErEQwNJJZjvW2JDpECjp7wAAAABJRU5ErkJggg==);
  width: 36px;
  height: 36px;
}

.leaflet-retina .leaflet-control-layers-toggle {
  background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADQAAAA0CAYAAADFeBvrAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3QYfFjIBzYr7NQAAAB1pVFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAAB80lEQVRo3u2ZzY3CMBCFv1V2L5QA9WxJSAGEMgqBBigFvIcc4JQKlnJIEYR7OHMJHkXJJMTZ4IxzKIJ/A+ORx2NglZIkwXFZ+FVfEGBDhpGBfUCqjD4ADrwBFsFhfwBXAIMkLODsD4WBNB8FeX4JuATQkLYekR7P6N/EBqBhAJUc4FsAXQAOQN0AiCr/6M/sn5k8D/jHFz44wgxYBxBQNABXTdIKQL6+4JIHfABqfnB1BFD/wE0AtYwNgDJ6k+fkQD8AVQkQKOeIpJE8wN8AFZEQWr2NfQ0dGBJA2kAp4QFHNBpA3qSBvQC6oPOJDpBK+AIov48OsFOiAn4B7gMAAChiPYBFBJCfwF0APcCyBLIj7mG4E4B9gJoSFvAeQA2gBpANaN8LI7xTXqwJXAbY11CfyoC+Ab4A1wLa6n9EAL0B3AnMk/RL6M/sn5l8D/wEvh/APh/APj/APD/AAB/APL/AOz/ACD/D4D+HwD7PwD5fwD0/wDY/wGQ/wdA/w+A/R8A+X8A9P8A2P8BkP8HQP8PgP0fAPl/APT/ANj/AZD/B0D/D4D9HwD5fwD0/wDY/wGQ/wdA/w+A/R8A+X8A9P8A2P8BkP8HQP8PgP0fAPl/APT/ANj/AZD/B0D/D4D9HwD5fwD0/wDY/wGQ/wdA/w+A/R8A+H8A8v8A5P8ByP8DkP8HIP8PQP4fgP0/APl/APT/ANj/AZD/B0D/D4D9HwD5fwD0/wDY/wGQ/wdA/w+A/R8A+X8A9P8A2P8BkP8HQP8PgP0fAPl/APT/ANj/AZD/B0D/D4D9HwD5fwD0/wDY/wGQ/wdA/w+A/R8A+H8AOv4BiANp8v8ABAAAAABJRU5ErkJggg==);
  background-size: 26px 26px;
}

.leaflet-touch .leaflet-control-layers-toggle {
  width: 44px;
  height: 44px;
}

.leaflet-control-layers .leaflet-control-layers-list,
.leaflet-control-layers-expanded .leaflet-control-layers-toggle {
  display: none;
}

.leaflet-control-layers-expanded .leaflet-control-layers-list {
  display: block;
  position: relative;
}

.leaflet-control-layers-expanded {
  padding: 6px 10px 6px 6px;
  color: #333;
  background: #fff;
}

.leaflet-control-layers-scrollbar {
  overflow-y: scroll;
  overflow-x: hidden;
  padding-right: 5px;
}

.leaflet-control-layers-selector {
  margin-top: 2px;
  position: relative;
  top: 1px;
}

.leaflet-control-layers label {
  display: block;
  font-size: 13px;
  font-size: 1.08333em;
}

.leaflet-control-layers-separator {
  height: 0;
  border-top: 1px solid #ddd;
  margin: 5px -10px 5px -6px;
}

.leaflet-default-icon-path {
  background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABkAAAApCAYAAADAk4LOAAADM0lEQVR42u2WZ3ATQRSAV2tJrCRKdWIb2ygI0kLwBAQGG0gIJe69J+69d8s+2+62K3K2EzMwM8lMGjPJJEMSmkBCCSHU/hqC/6n99JZz0qHTk4bEg0zPz3ubr28XYYyRNQYDYYQEoIQ8h8+GYNYQyBoKORPQYbIGE4JYQ1RriFYNwf8dBHWGxPJ/B8FrCNatwf8dBOcNcpLO7wBi1hB2DUGv0Bb4/wzC/8+Qd+v/E2SxfvwvXF0tn6TLkDg+tKXWMuzDSQhEEnAqAuAI+M0X4FgowCVwiH7GArQEfwsAsAY/uwccAx8cxW/9wNdCARqFPmyL8xA/TxD9ihfQZuhzX4Gf+gAoGqCL0Ieu4AsdwQ7gOXSE+9AB7E0X+APSwa4kD8qDfMiBPOiDHqg/MqAMSoRCKAEKoATKgFKoACqgHKiESqiCKqiGaqiBWqgDaqEG6qAe6qEeGqAB6qER6qEJmqAZmqAFWqAVWqANWqENOqAdOqATOqED+qE/3YB+dAd6oAd6oQd6oQ96oR+6oRu6oRu6oRt6oQd6oA96oB96oR96oRf6oB/6oBd6oYd6HdREHd2hFnqhG7qhB3qoF3d0h1ropXYddUMPdUFP6oIe1AWd0EkddEJ36IJ26IJ26IJO6IJu6Ka21AXd0A1d0EF9dEIndEIP9EA7tFMb7dBOnbRDB3RDN7RCJ3RBN3TTHp3QCd3QCd20Rwu1URd0UTf0QBc0Ug/tphbaoCW1UQ80Qys0QzM00h5t0Iot1EorNNMu6qZ9tEMLddM+6qF9dII6qZMO0AHqpoPUSft0gnZpN3VTD3VSF3XTbtpNXdRNndRFe2gHdVI7dVMP7aFO6qVu2ks91E39tI866Y9/rIe6qJvaaQ8do13UQ7upg/ZQHX2o+0Ed0EndtE376Id/d0A79UM79EE/9EEbtEMPtUMH9EFHaoM26KJtdJS6qAO6oIe6oQO6oYd6aBu0Uzv0QD8NpB7qpYHUTn00iPqok/ppL3VSH/VRN/VRN/VSDw2g/dRB/dRF/dRJfdRFfdRNPdRLB6iHDlA39dNe6qd+6qf+VEd7aQAN0AAaRJ10gNqoj/rpD+ylPtpHPdRP/amXDtIfJf4C3kkL2RkABNsAAAAASUVORK5CYII=);
}

.leaflet-container .leaflet-control-attribution {
  background: #fff;
  background: rgba(255, 255, 255, 0.8);
  margin: 0;
}

.leaflet-control-attribution,
.leaflet-control-scale-line {
  padding: 0 5px;
  color: #333;
  line-height: 1.4;
}

.leaflet-control-attribution a {
  text-decoration: none;
}

.leaflet-control-attribution a:hover,
.leaflet-control-attribution a:focus {
  text-decoration: underline;
}

.leaflet-attribution-flag {
  display: inline !important;
  vertical-align: baseline !important;
  width: 1em;
  height: 0.6669em;
}

.leaflet-left .leaflet-control-scale {
  margin-left: 5px;
}

.leaflet-bottom .leaflet-control-scale {
  margin-bottom: 5px;
}

.leaflet-control-scale-line {
  border: 2px solid #777;
  border-top: none;
  line-height: 1.1;
  padding: 2px 5px 1px;
  white-space: nowrap;
  -moz-box-sizing: border-box;
  box-sizing: border-box;
  background: rgba(255, 255, 255, 0.8);
  text-shadow: 1px 1px #fff;
}

.leaflet-control-scale-line:not(:first-child) {
  border-top: 2px solid #777;
  border-bottom: none;
  margin-top: -2px;
}

.leaflet-control-scale-line:not(:first-child):not(:last-child) {
  border-bottom: 2px solid #777;
}

.leaflet-touch .leaflet-control-attribution,
.leaflet-touch .leaflet-control-layers,
.leaflet-touch .leaflet-bar {
  box-shadow: none;
}

.leaflet-touch .leaflet-control-layers,
.leaflet-touch .leaflet-bar {
  border: 2px solid rgba(0, 0, 0, 0.2);
  background-clip: padding-box;
}

.leaflet-popup {
  position: absolute;
  text-align: center;
  margin-bottom: 20px;
}

.leaflet-popup-content-wrapper {
  padding: 1px;
  text-align: left;
  border-radius: 12px;
}

.leaflet-popup-content {
  margin: 13px 24px 13px 20px;
  line-height: 1.3;
  font-size: 13px;
  font-size: 1.08333em;
  min-height: 1px;
}

.leaflet-popup-content p {
  margin: 17px 0;
  margin: 1.3em 0;
}

.leaflet-popup-tip-container {
  width: 40px;
  height: 20px;
  position: absolute;
  left: 50%;
  margin-top: -1px;
  margin-left: -20px;
  overflow: hidden;
  pointer-events: none;
}

.leaflet-popup-tip {
  width: 17px;
  height: 17px;
  padding: 1px;
  margin: -10px auto 0;
  pointer-events: auto;
  -webkit-transform: rotate(45deg);
  -moz-transform: rotate(45deg);
  -ms-transform: rotate(45deg);
  transform: rotate(45deg);
}

.leaflet-popup-content-wrapper,
.leaflet-popup-tip {
  background: white;
  color: #333;
  box-shadow: 0 3px 14px rgba(0, 0, 0, 0.4);
}

.leaflet-container a.leaflet-popup-close-button {
  position: absolute;
  top: 0;
  right: 0;
  border: none;
  text-align: center;
  width: 24px;
  height: 24px;
  font: 16px/24px Tahoma, Verdana, sans-serif;
  color: #757575;
  text-decoration: none;
  background: transparent;
}

.leaflet-container a.leaflet-popup-close-button:hover,
.leaflet-container a.leaflet-popup-close-button:focus {
  color: #585858;
}

.leaflet-popup-scrolled {
  overflow: auto;
}

.leaflet-oldie .leaflet-popup-content-wrapper {
  -ms-zoom: 1;
}

.leaflet-oldie .leaflet-popup-tip {
  width: 24px;
  margin: 0 auto;
  -ms-filter: "progid:DXImageTransform.Microsoft.Matrix(M11=0.70710678, M12=0.70710678, M21=-0.70710678, M22=0.70710678)";
  filter: progid:DXImageTransform.Microsoft.Matrix(M11=0.70710678, M12=0.70710678, M21=-0.70710678, M22=0.70710678);
}

.leaflet-oldie .leaflet-control-zoom,
.leaflet-oldie .leaflet-control-layers,
.leaflet-oldie .leaflet-popup-content-wrapper,
.leaflet-oldie .leaflet-popup-tip {
  border: 1px solid #999;
}

.leaflet-div-icon {
  background: #fff;
  border: 1px solid #666;
}

.leaflet-tooltip {
  position: absolute;
  padding: 6px;
  background-color: #fff;
  border: 1px solid #fff;
  border-radius: 3px;
  color: #222;
  white-space: nowrap;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  user-select: none;
  pointer-events: none;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.4);
}

.leaflet-tooltip.leaflet-interactive {
  cursor: pointer;
  pointer-events: auto;
}

.leaflet-tooltip-top:before,
.leaflet-tooltip-bottom:before,
.leaflet-tooltip-left:before,
.leaflet-tooltip-right:before {
  position: absolute;
  pointer-events: none;
  border: 6px solid transparent;
  background: transparent;
  content: "";
}

.leaflet-tooltip-bottom {
  margin-top: 6px;
}

.leaflet-tooltip-top {
  margin-top: -6px;
}

.leaflet-tooltip-bottom:before,
.leaflet-tooltip-top:before {
  left: 50%;
  margin-left: -6px;
}

.leaflet-tooltip-top:before {
  bottom: 0;
  margin-bottom: -12px;
  border-top-color: #fff;
}

.leaflet-tooltip-bottom:before {
  top: 0;
  margin-top: -12px;
  margin-left: -6px;
  border-bottom-color: #fff;
}

.leaflet-tooltip-left {
  margin-left: -6px;
}

.leaflet-tooltip-right {
  margin-left: 6px;
}

.leaflet-tooltip-left:before,
.leaflet-tooltip-right:before {
  top: 50%;
  margin-top: -6px;
}

.leaflet-tooltip-left:before {
  right: 0;
  margin-right: -12px;
  border-left-color: #fff;
}

.leaflet-tooltip-right:before {
  left: 0;
  margin-left: -12px;
  border-right-color: #fff;
}

@media print {
  .leaflet-control {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }
}
`;

let stylesInjected = false;

/**
 * Injects Leaflet CSS styles into the document head.
 * This is called automatically when the Map component mounts.
 */
export function injectLeafletStyles(): void {
  if (stylesInjected || typeof document === "undefined") {
    return;
  }

  const styleId = "ivy-leaflet-styles";

  // Check if styles are already injected
  if (document.getElementById(styleId)) {
    stylesInjected = true;
    return;
  }

  const styleElement = document.createElement("style");
  styleElement.id = styleId;
  styleElement.textContent = LEAFLET_CSS;
  document.head.appendChild(styleElement);

  stylesInjected = true;
}
