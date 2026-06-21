import React, {
    useEffect,
    useRef,
    useState
} from 'react';

import './index.css';

import Lobby from './components/Lobby';
import GameBoard from './components/GameBoard';
import PlayerPanel from './components/PlayerPanel';

import {
    createConnection,
    createRoom
} from './api/signalr';

export default function App() {
    const [roomCode, setRoomCode] = useState(null);

    const [room, setRoom] = useState(null);

    const [error, setError] = useState(null);

    const [inputCode, setInputCode] = useState('');

    const connectionRef = useRef(null);

    useEffect(() => {
        const params =
            new URLSearchParams(
                window.location.search
            );

        const code =
            params.get('room');

        if (code) {
            setRoomCode(code);
        }
    }, []);


    useEffect(() => {
        if (!roomCode) return;

        const connection =
            createConnection();

        connectionRef.current =
            connection;

        connection.on(
            'RoomUpdated',
            (updatedRoom) => {
                setRoom(updatedRoom);
                setError(null);
            }
        );

        connection.on(
            'GameError',
            (message) => {
                setError(message);

                setTimeout(() => {
                    setError(null);
                }, 4000);
            }
        );

        const start =
            async () => {
                try {
                    await connection.start();

                    await connection.invoke(
                        'JoinRoom',
                        roomCode
                    );
                } catch {
                    setError(
                        'Ошибка подключения к серверу'
                    );
                }
            };

        start();

        return () => {
            connection.stop();
        };
    }, [roomCode]);

    const copyRoomLink =
        async () => {
            try {
                await navigator.clipboard.writeText(
                    window.location.href
                );
            } catch {
                setError(
                    'Не удалось скопировать ссылку'
                );
            }
        };

    const handleCreateRoom =
        async () => {
            try {
                const data =
                    await createRoom();

                if (!data.roomId)
                    return;

                window.history.pushState(
                    {},
                    '',
                    `?room=${data.roomId}`
                );

                setRoomCode(
                    data.roomId
                );

                await copyRoomLink();
            } catch {
                setError(
                    'Не удалось создать комнату'
                );
            }
        };

    const handleJoinRoom =
        (e) => {
            e.preventDefault();

            const code =
                inputCode.trim();

            if (!code) return;

            window.history.pushState(
                {},
                '',
                `?room=${code}`
            );

            setRoomCode(code);
        };

    const handleCellClick =
        (index) => {
            connectionRef.current?.invoke(
                'MakeMove',
                roomCode,
                index
            );
        };

    const handleRestart =
        () => {
            connectionRef.current?.invoke(
                'RestartGame',
                roomCode
            );
        };

    if (!roomCode || !room) {
        return (
            <Lobby
                inputCode={inputCode}
                setInputCode={setInputCode}
                onCreateRoom={handleCreateRoom}
                onJoinRoom={handleJoinRoom}
            />
        );
    }

    const cells =
        room.boardState.split('');

    const isMyTurn =
        room.isGameActive &&
        room.currentTurn ===
            room.role;

    const isBoardDisabled =
        !isMyTurn ||
        !room.isGameActive;

    return (
        <div className="game-container">
            {error && (
                <div className="error-toast">
                    ⚠️ {error}
                </div>
            )}

            <div className="scoreboard">
                Счёт:
                {' '}
                {room.creatorWins}
                {' '}
                (X)
                {' : '}
                {room.draws}
                {' : '}
                {room.guestWins}
                {' '}
                (O)
            </div>

            <div
                className="room-info"
                style={{
                    marginBottom: '20px'
                }}
            >
                <strong>
                    Комната:
                </strong>
                {' '}
                {room.roomId}
                {' | '}

                <button
                    onClick={copyRoomLink}
                    style={{
                        marginLeft: '10px',
                        cursor: 'pointer'
                    }}
                >
                    Копировать ссылку
                </button>
            </div>

            <div
                style={{
                    marginBottom: '20px',
                    fontSize: '18px'
                }}
            >
                Вы играете за:
                {' '}
                <strong>{room.role}</strong>
                {' | '}

                {!room.hasGuest ? (
                    <span>
                        ⏳ Ожидание подключения второго игрока...
                    </span>
                ) : room.isGameActive ? (
                    isMyTurn ? (
                        <span>
                            🟢 Твой ход
                        </span>
                    ) : (
                        <span>
                            ⏳ Ждем ход соперника
                        </span>
                    )
                ) : (
                    <span>
                        ⏸️ Партия завершена
                    </span>
                )}
            </div>

            <div className="layout">
                <PlayerPanel
                    title="Игрок X (Создатель)"
                    online={
                        room.hasCreator
                    }
                    active={
                        room.isGameActive &&
                        room.currentTurn ===
                            'X'
                    }
                />

                <GameBoard
                    cells={cells}
                    disabled={
                        isBoardDisabled
                    }
                    onCellClick={
                        handleCellClick
                    }
                />

                <PlayerPanel
                    title="Игрок O (Гость)"
                    online={
                        room.hasGuest
                    }
                    active={
                        room.isGameActive &&
                        room.currentTurn ===
                            'O'
                    }
                />
            </div>

            {!room.isGameActive &&
                room.hasGuest && (
                    <div
                        style={{
                            marginTop:
                                '20px',
                            textAlign:
                                'center'
                        }}
                    >
                        <h2>
                            {room.winner ===
                                'Draw' &&
                                '🤝 Ничья!'}

                            {room.winner ===
                                'X' &&
                                '🎉 Победил X!'}

                            {room.winner ===
                                'O' &&
                                '🎉 Победил O!'}
                        </h2>

                        <button
                            onClick={
                                handleRestart
                            }
                            style={{
                                padding:
                                    '10px 20px',
                                fontSize:
                                    '18px',
                                cursor:
                                    'pointer',
                                fontWeight:
                                    'bold'
                            }}
                        >
                            Следующая партия
                        </button>
                    </div>
                )}
        </div>
    );
}