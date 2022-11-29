// See https://aka.ms/new-console-template for more information

/*
 * This is the very start of what I hope to be a pretty cool REPL.
 *
 * Due to the language's nature, we'll easily be able to extend it with functions related to
 * the REPL.
 *
 * Such as:
 *   Setting the languages configuration in real-time from the language itself.
 *
 * Not going to write too much, as it's not even turing complete yet.
 */

using DL.Repl.Application;

new Repl().Execute();
