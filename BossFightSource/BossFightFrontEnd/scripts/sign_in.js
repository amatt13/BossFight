const signInButton = document.getElementById("signInButton");

signInButton.addEventListener("click", function() {
    SignInButtonClicked();
})

async function SignInButtonClicked() {
    errorOccured = false;

    const userNameInput = document.getElementById("inputUserName");
    const passwordInput = document.getElementById("inputPassword");

    const userNameText = String(userNameInput.value).trim();
    const passwordText = String(passwordInput.value).trim();

    if (userNameText.length <= 0 || userNameText.length > 100) {
        BlinkDiv("inputUserName")
        errorOccured = true;
    }
    if (passwordText.length <= 0 || passwordText.length > 100) {
        BlinkDiv("inputPassword")
        errorOccured = true;
    }

    if (!errorOccured)
    {
        await SendSignInRequest(userNameText, passwordText);
    }
}