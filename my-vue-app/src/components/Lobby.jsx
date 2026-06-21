export default function Lobby({
    inputCode,
    setInputCode,
    onCreateRoom,
    onJoinRoom
}) {
    return (
        <div className="lobby-container">
            <h1>Крестики-Нолики</h1>

            <button
                onClick={onCreateRoom}
                style={{
                    marginBottom: '20px',
                    display: 'block',
                    width: '100%',
                    padding: '12px',
                    cursor: 'pointer'
                }}
            >
                Создать комнату
            </button>

            <form onSubmit={onJoinRoom}>
                <input
                    type="text"
                    placeholder="Код комнаты"
                    value={inputCode}
                    onChange={(e) =>
                        setInputCode(e.target.value)
                    }
                />

                <button type="submit">
                    Войти
                </button>
            </form>
        </div>
    );
}