let timerElement = document.getElementById("timer");
let resendButton = document.getElementById("resendOtp");
let formElement = document.getElementById("otpForm");

// Initial timer in seconds (3 minutes)
let timerDuration = 180;
let interval;

function startTimer() {
    interval = setInterval(() => {
        if (timerDuration <= 0) {
            clearInterval(interval);
            resendButton.disabled = false; // Enable resend button after timer ends
            timerElement.textContent = "00:00";
        } else {
            let minutes = Math.floor(timerDuration / 60);
            let seconds = timerDuration % 60;
            timerElement.textContent =
                (minutes < 10 ? "0" + minutes : minutes) +
                ":" +
                (seconds < 10 ? "0" + seconds : seconds);
            timerDuration--;
        }
    }, 1000);
}

// Resend OTP logic
resendButton.addEventListener("click", () => {
    resendButton.disabled = true; // Disable button until next timer
    timerDuration = 180; // Reset timer
    startTimer();

    const resendOtpUrl = document.getElementById("resendOtpUrl").value;
    const email = document.getElementById("userEmail").value;

    // Make an AJAX call to resend OTP
    fetch(resendOtpUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ email: email }),
    })
        .then((response) => response.json())
        .then((data) => {
            if (data.success) {
                alert("A new OTP has been sent to your email.");
            } else {
                alert(data.message || "Failed to resend OTP. Please try again later.");
            }
        })
        .catch((error) => {
            console.error("Error:", error);
            alert("Something went wrong!");
        });
});

// Clear timer on form submit
formElement.addEventListener("submit", () => {
    clearInterval(interval);
});

// Start the initial timer
startTimer();
