#pragma once
#include <iostream>
#include <SDL2/SDL.h>
#include "SDL2/SDL_ttf.h"

class Game
{
  public:
    Game();
    ~Game();

    enum
    {
        NUM_KEYS = 256,
        NUM_BUTTONS = 3
    };

    typedef struct Key
    {
        bool down;
        bool pressed;  // Goes from not down to down
        bool released; // Goes from down to not down
    } Key;

    typedef struct Button
    {
        bool down;
        bool pressed;
        bool released;
    } Button;

    typedef struct Mouse
    {
        Button left;
        Button middle;
        Button right;
    } Mouse;

    // TODO: Use std::string instead of char
    // void init(const std::string &title, int x, int y, int width, int height);

    void init(const char *title, int x, int y, int width, int height);
    void handleEvents();
    void updateState();
    void renderFrame();
    void clean();
    bool running() { return isRunning; }

  private:
    void handleKeyboard();
    void handleMouse();
    void handleMouseButton(Button *b, uint32_t state);

    void getTextAndRect(int x, int y, const char *text, TTF_Font *font, SDL_Texture **texture, SDL_Rect *rect);
    void renderFrameCounter();

    bool isRunning;
    SDL_Window *window;
    SDL_Renderer *renderer;
    TTF_Font *font;

    // Game state
    uint32_t frameCount;
    Key keys[NUM_KEYS];
    Mouse mouse;
    int mouseX;
    int mouseY;

    // Model paramaters
    int translationX;
    int translationY;
    int rotationX;
    int rotationY;
    int scaleFactor;
};

// TODO: How to create SDL window with OpenGL context
//    https://www.youtube.com/watch?v=DkiKgQRiMRU&index=5&list=PLEETnX-uPtBXT9T-hD0Bj31DSnwio-ywh