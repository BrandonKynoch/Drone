//
//  AnalysisHandler.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/16/22.
//

import Foundation

class AnalysisHandler: ObservableObject {
    // SINGLETON PATTERN ///////////////////////////////////
    private static var privateSingleton: AnalysisHandler?
    public static var singleton: AnalysisHandler = {
        if privateSingleton == nil {
            privateSingleton = AnalysisHandler()
        }
        return privateSingleton!
    }()
    // SINGLETON PATTERN ///////////////////////////////////
    
    // UI FIELDS ///////////////////////////////////
    @Published public var weightOpacityScaler: CGFloat = 1
    // UI FIELDS ///////////////////////////////////
    
    init() {
        guard AnalysisHandler.privateSingleton == nil else {
            print("\nError: Attempted to initialize duplicate analysis handler\n")
            return
        }
        AnalysisHandler.privateSingleton = self
    }
}
