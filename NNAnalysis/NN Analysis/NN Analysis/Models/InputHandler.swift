//
//  InputHandler.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/18/22.
//

import Foundation
import SwiftUI

class InputHandler {
    // SINGLETON PATTERN ///////////////////////////////////
    private static var privateSingleton: InputHandler?
    public static var singleton: InputHandler = {
        if privateSingleton == nil {
            privateSingleton = InputHandler()
        }
        return privateSingleton!
    }()
    // SINGLETON PATTERN ///////////////////////////////////
    
    init() {
        guard InputHandler.privateSingleton == nil else {
            print("\nError: Attempted to initialize duplicate input handler\n")
            return
        }

        InputHandler.privateSingleton = self
    }
    
    static func HandleInput(keyCode: UInt16) {
        print(keyCode)
        
        // left = 123
        // up = 126
        // down = 125
        // right = 124
        
        // space = 49
        
        switch (keyCode) {
        case 123, 126: // Left or Up
            
            break
        case 124, 125: // Right or Down
            
            break
        default:
            break
        }
    }
}

struct KeyEventHandling: NSViewRepresentable {
    class KeyView: NSView {
        override var acceptsFirstResponder: Bool { true }
        override func keyDown(with event: NSEvent) {
//            print(">> key \(event.charactersIgnoringModifiers ?? "")")
            InputHandler.HandleInput(keyCode: event.keyCode)
        }
    }

    func makeNSView(context: Context) -> NSView {
        let view = KeyView()
        DispatchQueue.main.async { // wait till next event cycle
            view.window?.makeFirstResponder(view)
        }
        return view
    }

    func updateNSView(_ nsView: NSView, context: Context) {
    }
}

struct TestKeyboardEventHandling: View {
    var body: some View {
        Text("Hello, World!")
            .background(KeyEventHandling())
    }
}
