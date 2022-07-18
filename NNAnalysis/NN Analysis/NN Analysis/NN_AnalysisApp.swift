//
//  NN_AnalysisApp.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 2022/07/10.
//

import SwiftUI

@main
struct NN_AnalysisApp: App {
    init() {
        let _ = PrefsHandler.singleton
        let _ = DataHandler.singleton
        let _ = AnalysisHandler.singleton
        let _ = InputHandler.singleton
    }
    
    var body: some Scene {
        WindowGroup {
            MainView()
                .environmentObject(DataHandler.singleton)
                .environmentObject(AnalysisHandler.singleton)
                .environmentObject(PrefsHandler.singleton)
            //                .onAppear() {
            //                    for family in NSFontManager.shared.availableFontFamilies {
            //                        print(family)
            //                    }
            //                }
        }.commands {
            SidebarCommands()
        }
        
        Settings {
            PrefsMainView()
                .environmentObject(DataHandler.singleton)
                .environmentObject(AnalysisHandler.singleton)
                .environmentObject(PrefsHandler.singleton)
        }
    }
}
