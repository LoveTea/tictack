import { useEffect, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

export const useSignalR = (roomCode, onRoomUpdated, onError) => {
    const [connection, setConnection] = useState(null);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        if (!roomCode) return;

        // Настраиваем подключение к нашему .NET Хабу
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5039/gamehub', {
                // Обязательно для CORS, так как мы разрешили credentials на бэке
                withCredentials: true 
            })
            .withAutomaticReconnect() // Авто-переподключение при моргании интернета
            .build();

        setConnection(newConnection);

        return () => {
            if (newConnection) {
                newConnection.stop();
            }
        };
    }, [roomCode]);

    useEffect(() => {
        if (!connection) return;

        connection.start()
            .then(() => {
                setIsConnected(true);
                // Подключаемся к конкретной комнате сразу после установки связи
                connection.invoke('JoinRoom', roomCode);

                // Слушаем события от бэка
                connection.on('RoomUpdated', onRoomUpdated);
                connection.on('GameError', onError);
            })
            .catch(err => {
                console.error('SignalR Connection Error: ', err);
                onError('Не удалось подключиться к серверу игры.');
            });

        return () => {
            connection.off('RoomUpdated');
            connection.off('GameError');
        };
    }, [connection, roomCode, onRoomUpdated, onError]);

    // Метод отправки хода на бэк
    const makeMove = useCallback((cellIndex) => {
        if (connection && isConnected) {
            connection.invoke('MakeMove', roomCode, cellIndex).catch(err => console.error(err));
        }
    }, [connection, isConnected, roomCode]);

    // Метод запроса на перезапуск партии
    const restartGame = useCallback(() => {
        if (connection && isConnected) {
            connection.invoke('RestartGame', roomCode).catch(err => console.error(err));
        }
    }, [connection, isConnected, roomCode]);

    return { isConnected, makeMove, restartGame };
};