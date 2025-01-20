using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GomokuBot.AI
{

    public interface IEvaluate
    {
        double evaluateBoard(GomokuBot.Board board, Boolean IsMyturn);
        int getScore(GomokuBot.Board board);

    }

}