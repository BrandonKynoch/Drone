//
//  PrefsHandler.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/18.
//

import Foundation

class PrefsHandler: ObservableObject {
    // SINGLETON PATTERN ///////////////////////////////////
    private static var privateSingleton: PrefsHandler?
    public static var singleton: PrefsHandler = {
        if privateSingleton == nil {
            privateSingleton = PrefsHandler()
        }
        return privateSingleton!
    }()
    // SINGLETON PATTERN ///////////////////////////////////
    
    // CONSTANTS //////////////////////////////////////
    public static let LOAD_ALL_DRONES_KEY = "LoadAllDrones"
    // CONSTANTS //////////////////////////////////////
    
    @Published var loadAllDrones: Bool = false {
        didSet {
            UserDefaults.standard.set(loadAllDrones, forKey: PrefsHandler.LOAD_ALL_DRONES_KEY)
        }
    }
    
    init() {
        guard PrefsHandler.privateSingleton == nil else {
            print("\nError: Attempted to initialize duplicate prefs handler\n")
            return
        }
        
        loadAllDrones = UserDefaults.standard.bool(forKey: PrefsHandler.LOAD_ALL_DRONES_KEY)
        
        PrefsHandler.privateSingleton = self
    }
}
