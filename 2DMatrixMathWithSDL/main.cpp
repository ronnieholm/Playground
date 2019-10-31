#include "game.h"
#include "matrix.h"

int main(int argc, char **argv)
{
    const int FPS = 60;
    const int FRAME_DELAY = 1000 / FPS;

    Game game;
    game.init("Hello", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 512, 512);

    while (game.running())
    {
        uint32_t frameStart = SDL_GetTicks();
        game.handleEvents();
        game.updateState();
        game.renderFrame();
        int frameElapsed = SDL_GetTicks() - frameStart;

        if (FRAME_DELAY > frameElapsed)
        {
            SDL_Delay(FRAME_DELAY - frameElapsed);
        }
    }

    game.clean();
    return 0;
}

// auto a1 = std::make_unique<Matrix<double>>(1024, 1024);
// auto b1 = std::make_unique<Matrix<double>>(1024, 1024);
// auto c1 = a1->Multiply(*b1);

// void multiply()
// {
//     const int ROWS = 2;
//     const int COLUMNS = 2;

//     double x[] = {1, 2,
//                   3, 4};
//     double y[] = {5, 6,
//                   7, 8};

//     auto a = std::make_unique<Matrix<double>>(ROWS, COLUMNS, x);
//     auto b = std::make_unique<Matrix<double>>(ROWS, COLUMNS, y);
//     auto c = a->Multiply(*b);
//     c->Print();
// }
