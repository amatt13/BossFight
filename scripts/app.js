var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
const canvas = document.getElementById('mainCanvas');
const ctx = canvas.getContext('2d');
var monsterImage = new Image();
monsterImage.src = "images/sprites/monsters/goblin.png";
let initialMonsterImageX = 0, initialMMonsterImageY = -1;
monsterImage.addEventListener("load", () => {
    initialMMonsterImageY += monsterImage.height;
    ctx.drawImage(monsterImage, initialMonsterImageX, initialMMonsterImageY);
});
function TSButton() {
    document.getElementById("ts-example").innerHTML = greeter(user);
}
function SendTestGetRequest() {
    return __awaiter(this, void 0, void 0, function* () {
        const response = yield fetch("https://localhost:5001/WeatherForecast", {
            method: 'GET',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
        });
        if (!response.ok) { /* Handle */ }
        // If you care about a response:
        if (response.body !== null) {
            const body = yield response.json();
            console.log(body);
        }
    });
}
function Bla() {
    console.log("cake");
}
function ReSizeCanvas() {
    if (canvas.width != Math.floor(window.innerWidth * 0.33) || canvas.height != Math.floor(window.innerHeight * 0.66)) {
        canvas.width = Math.floor(window.innerWidth * 0.33);
        canvas.height = Math.floor(window.innerHeight * 0.66);
        initialMMonsterImageY = canvas.height / 2;
        monsterImageX = initialMonsterImageX, monsterImageY = initialMMonsterImageY;
        monsterImageMoveForward = true, monsterImageMoveDown = false;
        Bla();
    }
}
function Draw() {
    ReSizeCanvas(); // MUST be the first thing that happens
    ctx.globalCompositeOperation = 'destination-over';
    ctx.clearRect(0, 0, canvas.width, canvas.height); // clear canvas
    AnimateMonster();
}
let monsterImageX = initialMonsterImageX, monsterImageY = initialMMonsterImageY;
let monsterImageMoveForward = true, monsterImageMoveDown = false;
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
class Student {
    constructor(firstName, middleInitial, lastName) {
        this.firstName = firstName;
        this.middleInitial = middleInitial;
        this.lastName = lastName;
        this.fullName = firstName + " " + middleInitial + " " + lastName;
    }
}
function greeter(person) {
    return "Hello, " + person.firstName + " " + person.lastName;
}
let user = new Student("Fred", "M.", "Smith");
//# sourceMappingURL=app.js.map