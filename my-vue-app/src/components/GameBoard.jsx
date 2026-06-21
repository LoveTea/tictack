export default function GameBoard({
    cells,
    disabled,
    onCellClick
}) {
    return (
        <div className="board">
            {cells.map((cell, index) => (
                <button
                    key={index}
                    className="cell"
                    disabled={
                        disabled ||
                        cell !== '-'
                    }
                    onClick={() =>
                        onCellClick(index)
                    }
                >
                    {cell !== '-' ? cell : ''}
                </button>
            ))}
        </div>
    );
}