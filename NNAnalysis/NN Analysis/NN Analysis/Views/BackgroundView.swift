//
//  Background.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 2022/07/10.
//

import SwiftUI

struct BackgroundView: View {
    var body: some View {
        ZStack {
            Rectangle()
                .foregroundColor(S_COL_BACKGROUND)
        }
        .ignoresSafeArea()
    }
}

struct BackgroundView_Previews: PreviewProvider {
    static var previews: some View {
        BackgroundView()
    }
}
