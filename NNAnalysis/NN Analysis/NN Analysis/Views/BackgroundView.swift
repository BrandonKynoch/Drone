//
//  Background.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 2022/07/10.
//

import SwiftUI

struct BackgroundView: View {
    @State var viewMode: ViewMode
    
    var body: some View {
        ZStack {
            PanelView(type: .behindWindow)
            
            Rectangle()
                .foregroundColor((viewMode == .light ? S_COL_BACKGROUND2: S_COL_BACKGROUND).opacity(0.3))
        }
        .ignoresSafeArea()
    }
}

struct BackgroundView_Previews: PreviewProvider {
    static var previews: some View {
        BackgroundView(viewMode: .light)
    }
}
