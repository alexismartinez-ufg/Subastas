﻿const connection = new signalR.HubConnectionBuilder().withUrl("/SubastaHub").build();

connection.on("ReceiveBid", function (user, amount) {
    let formattedAmount = `$${parseFloat(amount).toFixed(2)}`;

    const bidTable = document.querySelector("#bid-table-body");
    const newRow = document.createElement("tr");
    newRow.innerHTML = `<td>${user}</td><td>${formattedAmount}</td><td>${new Date().toLocaleString()}</td>`;
    bidTable.prepend(newRow);

    document.querySelector("#current-amount").innerText = formattedAmount;
});

connection.on("ReceiveMessage", function (user, message) {
    // Actualiza el chat
    const chatDiv = document.querySelector("#chat-messages");
    const newMessage = document.createElement("div");
    newMessage.className = "message";
    newMessage.innerHTML = `<strong>${user}:</strong> ${message}`;
    chatDiv.appendChild(newMessage);
});

connection.on("UpdateParticipants", function (participants) {
    // Actualiza la lista de participantes
    const participantsTable = document.querySelector("#participants-table-body");
    participantsTable.innerHTML = "";
    participants.forEach(participant => {
        const newRow = document.createElement("tr");

        // Asegúrate de que participant.fechaSubasta sea una fecha en un formato que JavaScript pueda interpretar
        let fechaSubasta = new Date(participant.fechaSubasta);
        let fechaSubastaLocal = fechaSubasta.toLocaleString();

        newRow.innerHTML = `<td>${participant.nombreUsuario}</td><td>${participant.correoUsuario}</td><td>${fechaSubastaLocal}</td>`;
        participantsTable.appendChild(newRow);
    });
});

connection.start().then(() => {
    connection.invoke("UpdateParticipants", currentUser, true, currentSubastaId).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

document.querySelector(".btn-send-message").addEventListener("click", function (event) {
    sendMessage();
    event.preventDefault();
});

document.querySelector("#chat-input").addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        sendMessage();
        event.preventDefault();
    }
});

function sendMessage() {
    const message = document.querySelector("#chat-input").value;
    const user = currentUser;
    connection.invoke("SendMessage", user, message, currentSubastaId).catch(function (err) {
        return console.error(err.toString());
    });
    document.querySelector("#chat-input").value = "";
}


connection.on("ReceiveParticipants", function (participants) {
    const participantsTable = document.querySelector("#participants-table-body");
    participantsTable.innerHTML = '';
    participants.forEach(participant => {
        const newRow = document.createElement("tr");
        newRow.innerHTML = `<td>${participant.NombreUsuario}</td><td>${participant.CorreoUsuario}</td><td>${participant.FechaSubasta}</td>`;
        participantsTable.appendChild(newRow);
    });
});

window.addEventListener("beforeunload", function () {
    connection.invoke("UpdateParticipants", currentUser, false, currentSubastaId).catch(function (err) {
        return console.error(err.toString());
    });
});
