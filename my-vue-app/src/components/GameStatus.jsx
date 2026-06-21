export default function GameStatus({
    room,
    isMyTurn
}) {
    return (
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
    );
}