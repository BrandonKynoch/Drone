//
//  PrefsMainView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/17.
//

import SwiftUI

struct PrefsMainView: View {
    @EnvironmentObject var dataHandler: DataHandler
    @EnvironmentObject var prefsHandler: PrefsHandler
    
    var body: some View {
        ZStack {
            BackgroundView(viewMode: .dark)
            
            VStack {
                CustomToggleView(description: PrefsHandler.LOAD_ALL_DRONES_KEY, val: .init(get: { return prefsHandler.loadAllDrones }, set: { val in prefsHandler.loadAllDrones = val }))
                Spacer()
            }
            .padding()
        }
    }
}

struct PrefsMainView_Previews: PreviewProvider {
    static var previews: some View {
        PrefsMainView()
    }
}
