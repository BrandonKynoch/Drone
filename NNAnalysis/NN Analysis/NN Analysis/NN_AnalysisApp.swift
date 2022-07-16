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
        let _ = DataHandler.singleton
        let _ = AnalysisHandler.singleton
    }
    
    var body: some Scene {
        WindowGroup {
            MainView()
                .environmentObject(DataHandler.singleton)
                .environmentObject(AnalysisHandler.singleton)
            //                .onAppear() {
            //                    for family in NSFontManager.shared.availableFontFamilies {
            //                        print(family)
            //                    }
            //                }
        }.commands {
            SidebarCommands()
        }
    }
}
