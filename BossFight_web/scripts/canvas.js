function init() {
	window.requestAnimationFrame(ts_draw);
}

function ts_draw() {
	Draw()
	window.requestAnimationFrame(ts_draw);
}

init();
