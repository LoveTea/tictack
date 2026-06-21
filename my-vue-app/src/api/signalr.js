import * as signalR from '@microsoft/signalr';

const API_URL = 'http://localhost:5039';

export const createConnection = () =>
    new signalR.HubConnectionBuilder()
        .withUrl(`${API_URL}/gamehub`)
        .withAutomaticReconnect()
        .build();

export const createRoom = async () => {
    const response = await fetch(
        `${API_URL}/api/rooms/create`,
        {
            method: 'POST'
        }
    );

    return response.json();
};