export default function PlayerPanel({
    title,
    online,
    active
}) {
    return (
        <div
            className={`player-panel ${
                active ? 'active' : ''
            }`}
        >
            <h3>{title}</h3>

            <p>
                {online
                    ? 'В сети'
                    : 'Ожидание...'}
            </p>
        </div>
    );
}