#include <iostream>
#include "SDL2/SDL_ttf.h"
#include "game.h"

Game::Game()
{
    frameCount = 0;
    isRunning = false;
}

Game::~Game()
{
}

void Game::init(const char *title, int x, int y, int width, int height)
{
    if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
    {
        std::cerr << "Failed: " << SDL_GetError() << std::endl;
    }

    // TODO: instead of title, use title.c_str() because of C++ string
    window = SDL_CreateWindow(title, x, y, width, height, 0);
    if (!window)
    {
        std::cerr << "Failed: " << SDL_GetError() << std::endl;
    }

    renderer = SDL_CreateRenderer(window, 0, 0);
    if (!renderer)
    {
        std::cerr << "Failed: " << SDL_GetError() << std::endl;
    }

    if (TTF_Init() != 0)
    {
        std::cerr << "Failed: " << TTF_GetError() << std::endl;
    }

    font = TTF_OpenFont("FreeSans.ttf", 18);
    if (!font)
    {
        std::cerr << "Failed: " << TTF_GetError() << std::endl;
    }

    SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
    isRunning = true;
}

void Game::handleKeyboard()
{
    const uint8_t *sdlKeyState = SDL_GetKeyboardState(NULL);
    for (int i = 0; i < NUM_KEYS; i++)
    {
        bool down = sdlKeyState[i];
        bool wasDown = keys[i].down;
        keys[i].down = down;
        keys[i].pressed = down && !wasDown;
        keys[i].released = !down && wasDown;
    }
}

void Game::handleMouseButton(Button *b, uint32_t state)
{
    bool down = state;
    bool wasDown = b->down;
    b->down = down;
    b->pressed = down && !wasDown;
    b->released = !down && wasDown;
}

void Game::handleMouse()
{
    const uint32_t state = SDL_GetMouseState(&mouseX, &mouseY);
    handleMouseButton(&mouse.left, state & SDL_BUTTON(SDL_BUTTON_LEFT));
    handleMouseButton(&mouse.middle, state & SDL_BUTTON(SDL_BUTTON_MIDDLE));
    handleMouseButton(&mouse.right, state & SDL_BUTTON(SDL_BUTTON_RIGHT));
}

void Game::handleEvents()
{
    SDL_Event event;
    SDL_PollEvent(&event);

    switch (event.type)
    {
    case SDL_QUIT:
        isRunning = false;
        break;
    }

    handleKeyboard();
    handleMouse();
}

void Game::updateState()
{
    // TODO: Mouse hold down should translate
    //       Middle button should scale
    //       Right button should rotate
    //       Do same thing with arrow + mod keys
    //       Does something like imgui exist for SDL?

    frameCount++;
    //printf("%d\n", frameCount);

    if (keys[SDL_SCANCODE_ESCAPE].pressed)
    {
        printf("Escape pressed\n");
    }

    if (keys[SDL_SCANCODE_ESCAPE].released)
    {
        printf("Escape released\n");
    }

    if (keys[SDL_SCANCODE_X].down)
    {
        printf("x down\n");
        translationX += 1;
    }
    if (keys[SDL_SCANCODE_X].down && keys[SDL_SCANCODE_LSHIFT].down)
    {
        printf("S-x down\n");
        translationX -= 1;
    }
}

void Game::getTextAndRect(int x, int y, const char *text, TTF_Font *font, SDL_Texture **texture, SDL_Rect *rect)
{
    SDL_Color textColor = {255, 255, 255, 0};
    SDL_Surface *surface = TTF_RenderText_Solid(font, text, textColor);
    *texture = SDL_CreateTextureFromSurface(renderer, surface);
    int textWidth = surface->w;
    int textHeight = surface->h;
    SDL_FreeSurface(surface);
    rect->x = x;
    rect->y = y;
    rect->w = textWidth;
    rect->h = textHeight;
}

void Game::renderFrameCounter()
{
    SDL_Rect rect;
    SDL_Texture *texture;

    char buffer[100];
    sprintf(buffer, "%d", frameCount);
    getTextAndRect(10, 10, buffer, font, &texture, &rect);
    int success = SDL_RenderCopy(renderer, texture, NULL, &rect);
    if (success != 0)
    {
        printf("Failed: %s\n", SDL_GetError());
    }
    SDL_DestroyTexture(texture);
}

void Game::renderFrame()
{
    SDL_RenderClear(renderer);

    renderFrameCounter();



    SDL_RenderPresent(renderer);
}

void Game::clean()
{
    SDL_DestroyRenderer(renderer);
    SDL_DestroyWindow(window);
    TTF_CloseFont(font);
    TTF_Quit();
    SDL_Quit();
}