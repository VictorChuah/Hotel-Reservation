

// Initiate GET request to url provided
$('[data-get]').click(e => {
    e.preventDefault();
    let url = $(e.target).data('get');
    location = url || location;
});


// Escape regular expression
function escapeRegExp(string) {
    return string.replace(/[.*+\-?^${}()|[\]\\]/g, '\\$&');
}


// Auto-upper
$('[data-upper]').on('input', e => {
    let a = e.target.selectionStart;
    let b = e.target.selectionEnd;
    e.target.value = e.target.value.toUpperCase();
    e.target.setSelectionRange(a, b);
});

// Initiate POST request to url provided
$('[data-post]').click(e => {
    e.preventDefault();
    let url = $(e.target).data('post');

    let f = $('<form>')[0];
    f.method = 'post';
    f.action = url || location;
    $(document.body).append(f);
    f.submit();
});

// Reset form
$('[type=reset]').click(e => {
    e.preventDefault();
    location = location;
});

// Display Clock
function DisplayClock() {
    var currentTime = updateClock();
    document.getElementById("clock").firstChild.nodeValue = currentTime;
}

// Update clock 
function updateClock() {

    var currentTime = new Date();

    var currentHours = currentTime.getHours();
    var currentMinutes = currentTime.getMinutes();
    var currentSeconds = currentTime.getSeconds();
    var currentDay = currentTime.getDay();
    var currentMonth = currentTime.getMonth();

    // Pad the minutes and seconds with leading zeros, if required
    currentMinutes = (currentMinutes < 10 ? "0" : "") + currentMinutes;
    currentSeconds = (currentSeconds < 10 ? "0" : "") + currentSeconds;

    // Choose either "AM" or "PM" as appropriate
    var timeOfDay = (currentHours < 12) ? "AM" : "PM";

    // Convert the hours component to 12-hour format if needed
    currentHours = (currentHours > 12) ? currentHours - 12 : currentHours;

    // Convert an hours component of "0" to "12"
    currentHours = (currentHours == 0) ? 12 : currentHours;

    // Compose the string for display
    var currentTimeString = currentHours + ":" + currentMinutes + ":" + currentSeconds + " " + timeOfDay;

    return currentTimeString;
}

