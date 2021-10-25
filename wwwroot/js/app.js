var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var canvas = document.getElementById('mainCanvas');
var ctx = canvas.getContext('2d');
var monsterImage = new Image();
monsterImage.src = "images/sprites/monsters/goblin.png";
var initialMonsterImageX = 0, initialMMonsterImageY = -1;
monsterImage.addEventListener("load", function () {
    initialMMonsterImageY += monsterImage.height;
    ctx.drawImage(monsterImage, initialMonsterImageX, initialMMonsterImageY);
});
// TEST GET
function SendTestGETRequest() {
    return __awaiter(this, void 0, void 0, function () {
        var response, body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, fetch("https://localhost:5001/WeatherForecast", {
                        method: 'GET',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
                    })];
                case 1:
                    response = _a.sent();
                    if (!response.ok) { /* Handle */ }
                    if (!(response.body !== null)) return [3 /*break*/, 3];
                    return [4 /*yield*/, response.json()];
                case 2:
                    body = _a.sent();
                    console.log(body);
                    _a.label = 3;
                case 3: return [2 /*return*/];
            }
        });
    });
}
// TEST POST
function SendTestPOSTRequest() {
    return __awaiter(this, void 0, void 0, function () {
        var obj, response, body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    obj = { id: 101, testString: 'fucking ', unusedString: 'yes' };
                    return [4 /*yield*/, fetch("https://localhost:5001/PostTest", {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json; charset=UTF-8' },
                            body: JSON.stringify(obj)
                        })];
                case 1:
                    response = _a.sent();
                    if (!response.ok) { /* Handle */ }
                    if (!(response.body !== null)) return [3 /*break*/, 3];
                    return [4 /*yield*/, response.json()];
                case 2:
                    body = _a.sent();
                    console.log(body);
                    _a.label = 3;
                case 3: return [2 /*return*/];
            }
        });
    });
}
function ReSizeCanvas() {
    if (canvas.width != Math.floor(window.innerWidth * 0.33) || canvas.height != Math.floor(window.innerHeight * 0.66)) {
        canvas.width = Math.floor(window.innerWidth * 0.33);
        canvas.height = Math.floor(window.innerHeight * 0.66);
        initialMMonsterImageY = canvas.height / 2;
        monsterImageX = initialMonsterImageX, monsterImageY = initialMMonsterImageY;
        monsterImageMoveForward = true, monsterImageMoveDown = false;
    }
}
function Draw() {
    ReSizeCanvas(); // MUST be the first thing that happens
    ctx.globalCompositeOperation = 'destination-over';
    ctx.clearRect(0, 0, canvas.width, canvas.height); // clear canvas
    AnimateMonster();
}
var monsterImageX = initialMonsterImageX, monsterImageY = initialMMonsterImageY;
var monsterImageMoveForward = true, monsterImageMoveDown = false;
function AnimateMonster() {
    if (initialMMonsterImageY == -1)
        initialMMonsterImageY = canvas.height / 2;
    if (monsterImageX >= initialMonsterImageX + 100) {
        monsterImageMoveForward = false;
    }
    else if (monsterImageX <= 0) {
        monsterImageMoveForward = true;
    }
    if (monsterImageY <= initialMMonsterImageY - 50) {
        monsterImageMoveDown = true;
    }
    else if (monsterImageY >= initialMMonsterImageY) {
        monsterImageMoveDown = false;
    }
    monsterImageX = monsterImageMoveForward ? monsterImageX + 0.5 : monsterImageX - 0.5;
    monsterImageY = monsterImageMoveDown ? monsterImageY + 0.5 : monsterImageY - 0.5;
    ctx.drawImage(monsterImage, monsterImageX, monsterImageY);
    ctx.fillText('monsterImageY:' + monsterImageY.toString(), 300, 50);
    ctx.fillText('initialMMonsterImageY:' + initialMMonsterImageY.toString(), 300, 100);
    ctx.fillText('monsterImageMoveDown: ' + monsterImageMoveDown.toString(), 300, 150);
}
//# sourceMappingURL=app.js.map